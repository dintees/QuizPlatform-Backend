using AutoMapper;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Models.TestSession;
using QuizPlatform.Infrastructure.Models;

namespace QuizPlatform.Infrastructure.Services
{
    public class TestSessionService : ITestSessionService
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestSessionRepository _testSessionRepository;
        private readonly IMapper _mapper;

        public TestSessionService(ITestRepository testRepository, ITestSessionRepository testSessionRepository, IMapper mapper)
        {
            _testRepository = testRepository;
            _testSessionRepository = testSessionRepository;
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
            var testSession = await _testSessionRepository.GetByUserIdWithTestAsync(userId);
            return _mapper.Map<List<UserTestSessionDto>>(testSession);
        }

        public async Task<Result<TestDto?>> GetTestByTestSessionIdAsync(int testSessionId)
        {
            var testSession = await _testSessionRepository.GetBySessionIdAsync(testSessionId);
            if (testSession is null)
                return new Result<TestDto?> { Success = false, ErrorMessage = GeneralErrorMessages.NotFound };

            var test = await _testRepository.GetSetWithQuestionsByIdAsync(testSession.TestId);
            if (test is null)
                return new Result<TestDto?> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

            if (test.Questions is not null)
            {
                if (testSession.ShuffleQuestions)
                    ShuffleArray(test.Questions);

                foreach (var question in test.Questions)
                {
                    if (question.Answers is not null)
                    {
                        foreach (var answer in question.Answers)
                            answer.Correct = false;

                        if (testSession.ShuffleAnswers)
                            ShuffleArray(question.Answers);
                    }
                }
            }

            return new Result<TestDto?> { Success = true, Value = _mapper.Map<TestDto>(test) };
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
