using System.Globalization;
using AutoMapper;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.TestSession;
using QuizPlatform.Infrastructure.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

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

        public async Task<Result<TestSessionDto?>> GetTestByTestSessionIdAsync(int testSessionId, int userId)
        {
            var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId, true);
            if (testSession is null)
                return new Result<TestSessionDto?> { Success = false, ErrorMessage = GeneralErrorMessages.NotFound };

            if (testSession.UserId != userId)
                return new Result<TestSessionDto?> { Success = false, ErrorMessage = GeneralErrorMessages.Unauthorized };

            var userAnswers = await _userAnswersRepository.GetUserAnswersByTestSessionIdAsync(testSessionId);
            if (!testSession.IsCompleted && testSession.OneQuestionMode)
            {
                testSession.Test!.Questions = testSession.Test.Questions.Where(e => !(userAnswers!.Any(x => x.QuestionId == e.Id))).ToList();
            }
            List<UserAnswersDto>? correctAnswers = null;
            int score = 0;

            if (testSession.IsCompleted)
            {
                correctAnswers = GetCorrectAnswers(testSession);
                var userAnswersDto = new List<UserAnswersDto>();

                if (userAnswers is not null)
                {

                    foreach (var userAnswer in userAnswers)
                    {
                        var found = userAnswersDto.Find(e => e.QuestionId == userAnswer.QuestionId);
                        if (found is null)
                            userAnswersDto.Add(new UserAnswersDto { QuestionId = userAnswer.QuestionId, AnswerIds = userAnswer.QuestionAnswerId is null ? null : new List<int> { userAnswer.QuestionAnswerId!.Value }, ShortAnswerValue = userAnswer.ShortAnswerValue });
                        else
                            found.AnswerIds?.Add(userAnswer.QuestionAnswerId!.Value);
                    }
                }

                score = GetScoreResult(userAnswersDto, testSession);
                foreach (var correctAnswer in correctAnswers)
                {
                    correctAnswer.IsCorrect = userAnswersDto.Find(e => e.QuestionId == correctAnswer.QuestionId)?.IsCorrect ?? false;
                }
            }


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
            testDto.Score = score;
            testDto.MaxScore = questions?.Count ?? 0;

            return new Result<TestSessionDto?> { Success = true, Value = testDto };
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
                    testSession.Score = score;
                    testSession.MaxScore = testSession.Test!.Questions.Count(e => !e.IsDeleted);
                    testSession.IsCompleted = true;
                }
            }

            return await _testSessionRepository.SaveAsync();
        }

        public async Task<bool> SaveOneUserAnswersAsync(UserAnswersDto dto, int testSessionId, bool finish, int userId)
        {
            if (dto.ShortAnswerValue is null)
                foreach (var userSingleAnswer in dto.AnswerIds!)
                    await _userAnswersRepository.AddAsync(new UserAnswers { QuestionId = dto.QuestionId, QuestionAnswerId = userSingleAnswer, TestSessionId = testSessionId });
            else
                await _userAnswersRepository.AddAsync(new UserAnswers { QuestionId = dto.QuestionId, ShortAnswerValue = dto.ShortAnswerValue, TestSessionId = testSessionId });

            if (finish)
            {
                var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId);
                if (testSession != null)
                    testSession.IsCompleted = true;
            }

            return await _testSessionRepository.SaveAsync();
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

        private static int GetScoreResult(List<UserAnswersDto> userAnswersList, TestSession testSession)
        {
            int score = 0;
            var correctAnswers = GetCorrectAnswers(testSession);

            foreach (var userAnswers in userAnswersList)
            {
                if (userAnswers.ShortAnswerValue is not null)
                {
                    // shortAnswer string validation
                    var correctShortAnswersArr = correctAnswers.Find(e => e.QuestionId == userAnswers.QuestionId)?.ShortAnswerValue?.Split(", ");
                    var userAnswer = Regex.Replace(userAnswers.ShortAnswerValue, @"\s+", " ").Trim().ToLower();
                    userAnswer = ConvertFractions(userAnswer);

                    if (correctShortAnswersArr is not null && correctShortAnswersArr.Any(e => e.ToLower() == userAnswer))
                    {
                        userAnswers.IsCorrect = true;
                        score++;
                    }
                }
                else
                {
                    bool isCorrect = userAnswers.AnswerIds!.OrderBy(x => x).SequenceEqual(correctAnswers.Find(e => e.QuestionId == userAnswers.QuestionId)?.AnswerIds?.OrderBy(x => x)!);
                    if (isCorrect)
                    {
                        userAnswers.IsCorrect = true;
                        score++;
                    }
                }
            }

            return score;
        }

        private static string ConvertFractions(string input)
        {
            string pattern = @"(\d+)\s*/\s*(\d+)";
            string output = Regex.Replace(input, pattern, match =>
            {
                int numerator = int.Parse(match.Groups[1].Value);
                int denominator = int.Parse(match.Groups[2].Value);
                double result = (double)numerator / denominator;
                return result.ToString("0.##", CultureInfo.InvariantCulture);
            });

            return output;
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
