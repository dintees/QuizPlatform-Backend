using AutoMapper;
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

            var questions = GetQuestions();
            var tests = GetTests(questions);
            var userAnswers = GetUserAnswers(questions);
            var testSessions = GetTestSessions(tests);
            var testSessionRepositoryMock = GetTestSessionRepositoryMock(testSessions);

            var testRepositoryMock = new Mock<ITestRepository>();
            testRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync((int testId, bool _) => tests.FirstOrDefault(e => e.Id == testId));

            var userAnswersRepositoryMock = new Mock<IUserAnswersRepository>();
            userAnswersRepositoryMock.Setup(x => x.GetUserAnswersByTestSessionIdAsync(It.IsAny<int>())).ReturnsAsync(new List<UserAnswers>());
            userAnswersRepositoryMock.Setup(x => x.AddAsync(It.IsAny<UserAnswers>())).Callback(
                (UserAnswers answers) => userAnswers.Add(answers));

            userAnswersRepositoryMock.Setup(x => x.GetUserAnswersByTestSessionIdAsync(It.IsAny<int>())).ReturnsAsync((int testSessionId) => userAnswers.Where(e => e.TestSessionId == testSessionId).ToList());

            userAnswersRepositoryMock.Setup(x => x.Delete(It.IsAny<UserAnswers>())).Callback((UserAnswers userAnswersObject) => userAnswers.Remove(userAnswersObject));

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
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(result.Value, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Value);
            Assert.True(result.Success);
            Assert.NotNull(searchResult.Value);
            Assert.Equal("Test 1", searchResult.Value.Title);
            Assert.True(searchResult.Value.ShuffleQuestions);
        }

        [Fact]
        public async Task CreateTestSessionAsync_ForGivenDtoObjectWithDefaultOptionsProperties_ReturnsSpecificResultObjectWithInsertedId()
        {
            // Arrange
            const int userId = 1;
            var createTestSessionDto = new CreateTestSessionDto
            {
                TestId = 1,
                OneQuestionMode = false,
                ShuffleAnswers = true,
                ShuffleQuestions = true,
                UseDefaultTestOptions = true,
            };

            // Act
            var result = await _testSessionService.CreateTestSessionAsync(createTestSessionDto, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(result.Value, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Value);
            Assert.True(searchResult.Success);
            Assert.False(searchResult.Value?.ShuffleAnswers);
            Assert.True(searchResult.Value?.ShuffleQuestions);
            Assert.False(searchResult.Value?.OneQuestionMode);
        }

        [Fact]
        public async Task GetActiveUserTestSessionsAsync_ForGivenUserId_ReturnsActiveUserSessions()
        {
            // Arrange
            const int userId = 1;

            // Act
            var result = await _testSessionService.GetActiveUserTestSessionsAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTestByTestSessionIdAsync_ForGivenTestSessionIdAndUserIdForNotCompletedTest_ReturnsTestsWithoutCorrectAnswers()
        {
            // Arrange
            const int testSessionId = 1;
            const int userId = 1;

            // Act
            var result = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.False(result.Value.IsCompleted);
            Assert.Equal(0, result.Value.Score);
            Assert.Equal(0, result.Value.MaxScore);
            Assert.Equal("Test 1", result.Value.Title);
            Assert.Equal(2, result.Value.Questions?.Count);
            Assert.False(result.Value.ShuffleAnswers);
            Assert.True(result.Value.ShuffleQuestions);
            Assert.False(result.Value.OneQuestionMode);
            Assert.Null(result.Value.CorrectAnswers);
        }

        [Fact]
        public async Task GetTestByTestSessionIdAsync_ForCompletedTest_ReturnsTestsWithCorrectAnswersAndScore()
        {
            // Arrange
            const int testSessionId = 2;
            const int userId = 1;

            // Act
            var result = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            // Score: 1 / 2
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.IsCompleted);
            Assert.NotNull(result.Value.CorrectAnswers);
            Assert.Equal(1, result.Value.Score);
            Assert.Equal(2, result.Value.MaxScore);
            Assert.Equal("Test 1", result.Value.Title);
            Assert.Equal(2, result.Value.Questions?.Count);
            Assert.False(result.Value.ShuffleAnswers);
            Assert.True(result.Value.ShuffleQuestions);
            Assert.False(result.Value.OneQuestionMode);
        }

        [Fact]
        public async Task GetTestWithScore_ForDecimalShortAnswer_ReturnsTestsWithCorrectAnswersAndScore()
        {
            // Arrange
            const int testSessionId = 3;
            const int userId = 2;

            // Act
            var result = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            // 0.5 == "  0.50000  "
            // Score: 1 / 1
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.IsCompleted);
            Assert.Equal(1, result.Value.Score);
            Assert.Equal(1, result.Value.MaxScore);
        }

        [Fact]
        public async Task GetTestWithScore_ForValueAsAFractionShortAnswer_ReturnsTestsWithCorrectAnswersAndScore()
        {
            // Arrange
            const int testSessionId = 4;
            const int userId = 2;

            // Act
            var result = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            // 0.5 == "  1 /   2  "
            // Score: 1 / 1
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.IsCompleted);
            Assert.Equal(1, result.Value.Score);
            Assert.Equal(1, result.Value.MaxScore);
        }

        [Fact]
        public async Task SaveUserAnswersAsync_ForNotStartedTest_ShouldSaveAllUserAnswers()
        {
            // Arrange
            const int testSessionId = 5;
            const int userId = 2;
            var userAnswerDto = new List<UserAnswersDto>
            {
                new UserAnswersDto
                {
                    QuestionId = 3,
                    ShortAnswerValue = " 1 /  2 "
                }
            };

            // Act
            var result = await _testSessionService.SaveUserAnswersAsync(userAnswerDto, testSessionId, false, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult.Value);
            Assert.False(searchResult.Value.IsCompleted);
            Assert.Single(searchResult.Value.Questions!);
            Assert.Equal(" 1 /  2 ", searchResult.Value.Questions?[0].Answers?[0].Answer);
        }

        [Fact]
        public async Task SaveUserAnswersAsync_ForNotStartedTestAndFinishedImmediately_ShouldSaveAllUserAnswersAndCountScore()
        {
            // Arrange
            const int testSessionId = 5;
            const int userId = 2;
            var userAnswerDto = new List<UserAnswersDto>
            {
                new UserAnswersDto
                {
                    QuestionId = 3,
                    ShortAnswerValue = " 1 /  2 "
                }
            };

            // Act
            var result = await _testSessionService.SaveUserAnswersAsync(userAnswerDto, testSessionId, true, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult.Value);
            Assert.True(searchResult.Value.IsCompleted);
            Assert.Single(searchResult.Value.Questions!);
            Assert.Equal(" 1 /  2 ", searchResult.Value.Questions?[0].Answers?[0].Answer);
            Assert.Equal(1, searchResult.Value.Score);
            Assert.Equal(1, searchResult.Value.MaxScore);
        }

        [Fact]
        public async Task SaveUserAnswersAsync_ForTheStartedTest_ShouldSaveModifiedUserAnswers()
        {
            // Arrange
            const int testSessionId = 6;
            const int userId = 2;
            var userAnswerDto = new List<UserAnswersDto>
            {
                new UserAnswersDto
                {
                    QuestionId = 1,
                    AnswerIds = new List<int> { 1 }
                },
                // modify existing answer to other value
                new UserAnswersDto
                {
                    QuestionId = 2,
                    AnswerIds = new List<int> { 3 }
                }
            };

            // Act
            var result = await _testSessionService.SaveUserAnswersAsync(userAnswerDto, testSessionId, true, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult.Value);
            Assert.Equal(2, searchResult.Value.Questions?.Count);
            Assert.Equal(2, searchResult.Value.Score);
            Assert.Equal(2, searchResult.Value.MaxScore);
        }

        [Fact]
        public async Task SaveOneUserAnswersAsync_ForOneAnswersAndFinishEqualToFalse_SaveOneUserAnswer()
        {
            // Arrange
            var userAnswerDto = new UserAnswersDto
            {
                QuestionId = 1,
                AnswerIds = new List<int> { 1 }
            };
            const int testSessionId = 6;
            const int userId = 2;

            // Act
            var result = await _testSessionService.SaveOneUserAnswersAsync(userAnswerDto, testSessionId, false, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult.Value);
            Assert.Equal(2, searchResult.Value.Questions?.Count);
            Assert.Equal(0, searchResult.Value.Score);
            Assert.Equal(0, searchResult.Value.MaxScore);
        }

        [Fact]
        public async Task SaveOneUserAnswersAsync_ForSkippedAnswerAndFinishFlag_SaveOneUserAnswerAndCountScore()
        {
            // Arrange
            var userAnswerDto = new UserAnswersDto
            {
                QuestionId = 1,
                AnswerIds = new List<int>()
            };
            const int testSessionId = 6;
            const int userId = 2;

            // Act
            var result = await _testSessionService.SaveOneUserAnswersAsync(userAnswerDto, testSessionId, true, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult.Value);
            Assert.Equal(2, searchResult.Value.Questions?.Count);
            Assert.Equal(0, searchResult.Value.Score);
            Assert.Equal(2, searchResult.Value.MaxScore);
        }

        [Fact]
        public async Task SaveOneUserAnswersAsync_ForOneCorrectAnswersAndFinishFlag_SaveOneUserAnswerAndCountScore()
        {
            // Arrange
            var userAnswerDto = new UserAnswersDto
            {
                QuestionId = 1,
                AnswerIds = new List<int> { 1 }
            };
            const int testSessionId = 6;
            const int userId = 2;

            // Act
            var result = await _testSessionService.SaveOneUserAnswersAsync(userAnswerDto, testSessionId, true, userId);
            var searchResult = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId, userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult.Value);
            Assert.Equal(2, searchResult.Value.Questions?.Count);
            Assert.Equal(1, searchResult.Value.Score);
            Assert.Equal(2, searchResult.Value.MaxScore);
        }


        private static List<UserAnswers> GetUserAnswers(List<Question> questions)
        {
            return new List<UserAnswers>
            {
                new UserAnswers
                {
                    Id = 1, QuestionId = 1, Question = questions.FirstOrDefault(e => e.Id == 1), TestSessionId = 2,
                    QuestionAnswerId = 1, QuestionAnswer = new QuestionAnswer { Id = 1, Content = "A", Correct = true }
                },
                new UserAnswers
                {
                    Id = 2, QuestionId = 2, Question = questions.FirstOrDefault(e => e.Id == 2), TestSessionId = 2,
                    QuestionAnswerId = 2, QuestionAnswer = new QuestionAnswer { Id = 2, Content = "A", Correct = true }
                },
                new UserAnswers
                {
                    Id = 3, QuestionId = 3, Question = questions.FirstOrDefault(e => e.Id == 3), TestSessionId = 3,
                    ShortAnswerValue = "  0.5000   "
                },
                new UserAnswers
                {
                    Id = 4, QuestionId = 3, Question = questions.FirstOrDefault(e => e.Id == 3), TestSessionId = 4,
                    ShortAnswerValue = "  1 /   2   "
                },
                new UserAnswers
                {
                    Id = 5, QuestionId = 2, Question = questions.FirstOrDefault(e => e.Id == 2), TestSessionId = 6,
                    QuestionAnswerId = 2, QuestionAnswer = new QuestionAnswer { Id = 2, Content = "A", Correct = true }
                },
            };
        }

        private static List<Question> GetQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    Id = 1,
                    QuestionType = QuestionType.SingleChoice,
                    IsDeleted = false,
                    MathMode = false,
                    Content = "A",
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer { Id = 1, Content = "A", Correct = true },
                    }
                },
                new Question
                {
                    Id = 2,
                    QuestionType = QuestionType.SingleChoice,
                    IsDeleted = false,
                    MathMode = false,
                    Content = "B",
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer { Id = 2, Content = "A", Correct = false },
                        new QuestionAnswer { Id = 3, Content = "B", Correct = true },
                    }
                },
                new Question
                {
                    Id = 3,
                    QuestionType = QuestionType.ShortAnswer,
                    IsDeleted = false,
                    MathMode = false,
                    Content = "0.5?",
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer { Id = 4, Content = "0.5", Correct = true }
                    }
                }
            };
        }

        private static List<Test> GetTests(List<Question> questions)
        {
            return new List<Test>
            {
                new Test
                {
                    Id = 1,
                    Title = "Test 1",
                    Description = "Description 1",
                    IsDeleted = false,
                    IsPublic = true,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = true,
                    Questions = questions.Where(e => e.Id is 1 or 2).ToList()
                },
                new Test
                {
                    Id = 2,
                    Title = "Test 2",
                    Description = "Description 2",
                    IsDeleted = false,
                    IsPublic = true,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = false,
                    Questions = questions.Where(e => e.Id == 3).ToList()
                },
            };
        }

        private static List<TestSession> GetTestSessions(List<Test> tests)
        {
            return new List<TestSession>
            {
                new TestSession
                {
                    Id = 1,
                    IsCompleted = false,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = true,
                    TestId = 1,
                    Test = tests.FirstOrDefault(e => e.Id == 1),
                    UserId = 1
                },
                new TestSession
                {
                    Id = 2,
                    IsCompleted = true,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = true,
                    TestId = 1,
                    Test = tests.FirstOrDefault(e => e.Id == 1),
                    UserId = 1
                },
                new TestSession
                {
                    Id = 3,
                    IsCompleted = true,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = false,
                    TestId = 2,
                    Test = tests.FirstOrDefault(e => e.Id == 2),
                    UserId = 2
                },
                new TestSession
                {
                    Id = 4,
                    IsCompleted = true,
                    OneQuestionMode = true,
                    ShuffleAnswers = false,
                    ShuffleQuestions = false,
                    TestId = 2,
                    Test = tests.FirstOrDefault(e => e.Id == 2),
                    UserId = 2
                },
                new TestSession
                {
                    Id = 5,
                    IsCompleted = false,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = false,
                    TestId = 2,
                    Test = tests.FirstOrDefault(e => e.Id == 2),
                    UserId = 2
                },
                new TestSession()
                {
                    Id = 6,
                    IsCompleted = false,
                    OneQuestionMode = false,
                    ShuffleAnswers = false,
                    ShuffleQuestions = false,
                    TestId = 1,
                    Test = tests.FirstOrDefault(e => e.Id == 1),
                    UserId = 2
                }
            };
        }

        private static Mock<ITestSessionRepository> GetTestSessionRepositoryMock(List<TestSession> testSessions)
        {
            var testSessionRepositoryMock = new Mock<ITestSessionRepository>();
            testSessionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TestSession>())).Callback((TestSession testSession) =>
            {
                testSession.Id = 10;
                testSession.Test = GetTests(GetQuestions()).FirstOrDefault(e => e.Id == 1);
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

            testSessionRepositoryMock.Setup(x => x.GetBySessionIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(
                (int testSessionId, bool _) =>
                {
                    return testSessions.FirstOrDefault(e => e.Id == testSessionId);
                });

            testSessionRepositoryMock.Setup(x => x.GetByUserIdWithTestAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(
                (int userId, bool sortByModificationTime) =>
                {
                    if (sortByModificationTime)
                        return testSessions.Where(e => e.UserId == userId).OrderByDescending(e => e.TsUpdate).ToList();
                    return testSessions.Where(e => e.UserId == userId).ToList();
                });

            testSessionRepositoryMock.Setup(x => x.GetByTestIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(
                (int testId, bool _) =>
                {
                    return testSessions.Where(e => e.TestId == testId).ToList();
                });

            testSessionRepositoryMock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            return testSessionRepositoryMock;
        }
    }
}
