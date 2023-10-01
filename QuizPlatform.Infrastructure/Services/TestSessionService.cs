using AutoMapper;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Test;
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
            return _mapper.Map<List<UserTestSessionDto>>(testSession);
        }

        public async Task<bool> SaveUserAnswersAsync(List<UserAnswersDto> dto, int testSessionId, bool finish, int userId)
        {
            var currentlySavedEntities = await _userAnswersRepository.GetUserAnswersByTestSessionIdAsync(testSessionId);

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

            var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId);
            if (testSession is not null)
            {
                testSession.TsUpdate = DateTime.Now;
                if (finish) testSession.IsFinished = true;
            }

            return await _testSessionRepository.SaveAsync();
        }

        public async Task<Result<TestDto?>> GetTestByTestSessionIdAsync(int testSessionId, int userId)
        {
            var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId, true);
            if (testSession is null)
                return new Result<TestDto?> { Success = false, ErrorMessage = GeneralErrorMessages.NotFound };

            if (testSession.UserId != userId)
                return new Result<TestDto?> { Success = false, ErrorMessage = GeneralErrorMessages.Unauthorized };

            var userAnswers = await _userAnswersRepository.GetUserAnswersByTestSessionIdAsync(testSessionId);

            // Shuffle questions and answers
            if (testSession?.Test?.Questions is not null)
            {
                if (testSession.IsFinished == false && testSession.ShuffleQuestions)
                    ShuffleArray(testSession.Test.Questions);

                foreach (var question in testSession.Test.Questions)
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

                        if (testSession.IsFinished == false && testSession.ShuffleAnswers)
                            ShuffleArray(question.Answers);
                    }
                }
            }
            var testDto = _mapper.Map<TestDto>(testSession?.Test);

            return new Result<TestDto?> { Success = true, Value = testDto };
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
