using AutoMapper;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.TestSession;
using QuizPlatform.Infrastructure.Models;

namespace QuizPlatform.Infrastructure.Services
{
    public class TestSessionService : ITestSessionService
    {
        private readonly ITestSessionRepository _testSessionRepository;
        private readonly IUserAnswersRepository _userAnswersRepository;
        private readonly IMapper _mapper;

        public TestSessionService(ITestSessionRepository testSessionRepository, IUserAnswersRepository userAnswersRepository, IMapper mapper)
        {
            _testSessionRepository = testSessionRepository;
            _userAnswersRepository = userAnswersRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> CreateTestSession(CreateTestSessionDto dto, int userId)
        {
            var testSession = _mapper.Map<TestSession>(dto);
            testSession.UserId = userId;

            await _testSessionRepository.AddAsync(testSession);
            return await _testSessionRepository.SaveAsync() ? new Result<int> { Success = true, Value = testSession.Id } : new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
        }

        public async Task<List<UserTestSessionDto>> GetActiveUserTestSessionsAsync(int userId)
        {
            var testSession = await _testSessionRepository.GetByUserIdWithTestAsync(userId, true);
            return _mapper.Map<List<UserTestSessionDto>>(testSession.Where(e => e.Test?.IsDeleted == false || (e.Test?.IsDeleted == true && e.IsCompleted)).ToList());
        }

        public async Task<bool> SaveUserAnswersAsync(List<UserAnswersDto> dto, int testSessionId, bool finish, int userId)
        {
            var currentlySavedEntities = await _userAnswersRepository.GetUserAnswersByTestSessionIdAsync(testSessionId);

            // Insert/update user answers to UserAnswers entity
            foreach (var userAnswer in dto)
            {
                if (userAnswer.ShortAnswerValue is null)
                {
                    foreach (var userSingleAnswer in userAnswer.AnswerIds!)
                    {
                        var foundEntities = currentlySavedEntities?.Find(e => e.QuestionAnswerId == userSingleAnswer);
                        if (foundEntities is null)
                            await _userAnswersRepository.AddAsync(new UserAnswers { QuestionId = userAnswer.QuestionId, QuestionAnswerId = userSingleAnswer, TestSessionId = testSessionId });
                        else
                            currentlySavedEntities?.Remove(foundEntities);
                    }
                }
                else
                {
                    var foundEntities = currentlySavedEntities?.Find(e => e.QuestionId == userAnswer.QuestionId);
                    if (foundEntities is null)
                        await _userAnswersRepository.AddAsync(new UserAnswers { QuestionId = userAnswer.QuestionId, ShortAnswerValue = userAnswer.ShortAnswerValue, TestSessionId = testSessionId });
                    else
                        foundEntities.ShortAnswerValue = userAnswer.ShortAnswerValue;
                }
            }

            foreach (var entity in currentlySavedEntities!.Where(entity => entity.ShortAnswerValue is null))
                _userAnswersRepository.Delete(entity);

            var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId, true);
            if (testSession is not null)
            {
                testSession.TsUpdate = DateTime.Now;
                if (finish)
                {
                    var score = GetScoreResult(dto, testSession);
                    // TODO score to the database
                    testSession.IsCompleted = true;
                }
            }

            return await _testSessionRepository.SaveAsync();
        }

        private static int GetScoreResult(List<UserAnswersDto> dto, TestSession testSession)
        {
            int score = 0;
            var correctAnswers = GetCorrectAnswers(testSession);

            foreach (var userAnswers in dto)
            {
                if (userAnswers.ShortAnswerValue is not null)
                {
                    var correctShortAnswersArr = correctAnswers.Find(e => e.QuestionId == userAnswers.QuestionId)
                        ?.ShortAnswerValue?.Split(", ");

                    if (correctShortAnswersArr is not null &&
                        correctShortAnswersArr.Any(e => e == userAnswers.ShortAnswerValue))
                        score++;
                }
                else
                {
                    score += userAnswers.AnswerIds!.SequenceEqual(correctAnswers.Find(e => e.QuestionId == userAnswers.QuestionId)?.AnswerIds!) ? 1 : 0;
                }
            }

            return score;
        }

        public async Task<Result<TestSessionDto?>> GetTestByTestSessionIdAsync(int testSessionId, int userId)
        {
            var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId, true);
            if (testSession is null)
                return new Result<TestSessionDto?> { Success = false, ErrorMessage = GeneralErrorMessages.NotFound };

            if (testSession.UserId != userId)
                return new Result<TestSessionDto?> { Success = false, ErrorMessage = GeneralErrorMessages.Unauthorized };

            List<UserAnswersDto>? correctAnswers = null;

            if (testSession.IsCompleted) correctAnswers = GetCorrectAnswers(testSession);
            var userAnswers = await _userAnswersRepository.GetUserAnswersByTestSessionIdAsync(testSessionId);

            // Shuffle questions and answers
            var questions = testSession.Test?.Questions.Where(e => !e.IsDeleted).ToList();
            if (testSession.Test?.Questions is not null)
            {
                if (testSession.IsCompleted == false && testSession.ShuffleQuestions)
                    ShuffleArray(questions);

                if (questions != null)
                    foreach (var question in questions)
                    {
                        if (question.Answers is not null)
                        {
                            foreach (var answer in question.Answers)
                            {
                                if (question.QuestionType == QuestionType.ShortAnswer)
                                {
                                    question.Answers = new List<QuestionAnswer> { new QuestionAnswer { Content = userAnswers?.Find(e => e.QuestionId == question.Id)?.ShortAnswerValue ?? string.Empty, Correct = false } };
                                    continue;
                                }

                                answer.Correct = userAnswers!.Any(e => e.QuestionAnswerId == answer.Id);
                            }

                            if (testSession.IsCompleted == false && testSession.ShuffleAnswers)
                                ShuffleArray(question.Answers);
                        }
                    }
            }

            if (questions != null) testSession.Test!.Questions = questions;
            var testDto = _mapper.Map<TestSessionDto>(testSession.Test);
            testDto.OneQuestionMode = testSession.OneQuestionMode;
            testDto.IsCompleted = testSession.IsCompleted;
            testDto.CorrectAnswers = correctAnswers;

            return new Result<TestSessionDto?> { Success = true, Value = testDto };
        }

        private static List<UserAnswersDto> GetCorrectAnswers(TestSession testSession)
        {
            List<UserAnswersDto> correctAnswers = new();
            var correctAnswersFromEntity = testSession.Test!.Questions.Where(e => !e.IsDeleted);
            foreach (var question in correctAnswersFromEntity)
            {
                if (question.QuestionType == QuestionType.ShortAnswer)
                    correctAnswers.Add(new UserAnswersDto
                    {
                        QuestionId = question.Id,
                        ShortAnswerValue = string.Join(", ", question.Answers!.Select(e => e.Content))
                    });
                else
                    correctAnswers.Add(new UserAnswersDto
                    {
                        QuestionId = question.Id,
                        AnswerIds = question.Answers?.Where(e => e.Correct).Select(e => e.Id).ToList()
                    });
            }

            return correctAnswers;
        }

        private static void ShuffleArray<T>(ICollection<T>? coll)
        {
            if (coll is null)
                return;

            var rand = new Random();
            var array = new T[coll.Count];
            coll.CopyTo(array, 0);

            for (int i = array.Length - 1; i > 0; --i)
            {
                var j = rand.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }

            coll.Clear();
            foreach (var elem in array)
                coll.Add(elem);
        }
    }
}
