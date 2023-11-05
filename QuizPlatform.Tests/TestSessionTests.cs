using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.TestSession;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class TestSessionTests
    {
        private readonly ITestSessionService _testSessionService;

        public TestSessionTests()
        {
            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            var testSessions = GetTestSessions();
            var testSessionRepositoryMock = GetTestSessionRepositoryMock(testSessions);

            var testRepositoryMock = new Mock<ITestRepository>();

            var userAnswersRepositoryMock = new Mock<IUserAnswersRepository>();

            _testSessionService = new TestSessionService(testSessionRepositoryMock.Object, testRepositoryMock.Object, userAnswersRepositoryMock.Object, mapper);
        }

        [Fact]
        public async Task CreateTestSessionAsync_ForGivenDtoObject_ReturnsResultObjectWithInsertedId()
        {
            // Arrange
            const int userId = 1;
            var createTestSessionDto = new CreateTestSessionDto
            {
                TestId = 1,
                OneQuestionMode = false,
                ShuffleAnswers = true,
                ShuffleQuestions = true
            };

            // Act
            var result = await _testSessionService.CreateTestSessionAsync(createTestSessionDto, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Value);
        }

        [Fact]
        public async Task GetActiveUserTestSessionsAsync_ForGivenUserId_ReturnsActiveUserSessions()
        {
            // Arrange
            const int userId = 1;

            // Act
            var result = await _testSessionService.GetActiveUserTestSessionsAsync(userId);

            // Assert
            Assert.Equal(1, result.Count);
        }



        private List<TestSession> GetTestSessions()
        {
            return new List<TestSession>
            {
                new TestSession
                {
                    Id = 1,
                    IsCompleted = false,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = false,
                    TestId = 1,
                    Test = new Test
                    {
                        Id = 1,
                        Title = "Test 1",
                        Description = "Description 1",
                        IsDeleted = false,
                        IsPublic = true,
                        OneQuestionMode = false,
                        ShuffleAnswers = false,
                        ShuffleQuestions = false,
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                Id = 1,
                                QuestionType = QuestionType.SingleChoice,
                                IsDeleted = false,
                                MathMode = false,
                                Answers = new List<QuestionAnswer>
                                {
                                    new QuestionAnswer { Id = 1, Content = "A", Correct = true },
                                }
                            },
                        }
                    },
                    UserId = 1
                }
            };
        }

        private Mock<ITestSessionRepository> GetTestSessionRepositoryMock(List<TestSession> testSessions)
        {
            var testSessionRepositoryMock = new Mock<ITestSessionRepository>();
            testSessionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TestSession>())).Callback((TestSession testSession) =>
            {
                testSession.Id = 10;
                testSessions.Add(testSession);
            });

            testSessionRepositoryMock.Setup(x => x.GetByUserIdWithTestAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    (int userId, bool sortByModificationTime) =>
                    {
                        if (sortByModificationTime)
                            return testSessions.Where(e => e.UserId == userId).OrderBy(e => e.IsCompleted).ThenByDescending(e => e.TsUpdate).ToList();
                        return testSessions.Where(e => e.UserId == userId).ToList();
                    });

            testSessionRepositoryMock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            return testSessionRepositoryMock;
        }
    }
}
