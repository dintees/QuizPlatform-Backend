using AutoMapper;
using FluentValidation;
using Moq;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class TestTests
    {
        private readonly TestService _testService;

        public TestTests()
        {
            var tests = GetTests();
            var testRepositoryMock = GetSetRepositoryMock(tests);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            var questionRepositoryMock = new Mock<IQuestionRepository>();
            questionRepositoryMock.Setup(x => x.GetQuestionByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(
                (int id, bool _) =>
                {
                    return GetQuestions().FirstOrDefault(e => e.Id == id);
                });


            var testSessionRepositoryMock = new Mock<ITestSessionRepository>();
            var userAnswersRepositoryMock = new Mock<IUserAnswersRepository>();

            IValidator<Test> testValidator = new TestValidator();

            _testService = new TestService(mapper, testRepositoryMock.Object, questionRepositoryMock.Object, testSessionRepositoryMock.Object, userAnswersRepositoryMock.Object, testValidator);
        }

        [Fact]
        public async Task GetAllUserTestsAsync_ForGiverUserId_ReturnsTestsBelongToUser()
        {
            // Arrange
            const int userId = 5;

            // Act
            var result = await _testService.GetAllUserTestsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllUserTestsAsync_ForAdmin_ReturnsAllTests()
        {
            // Act
            var result = await _testService.GetAllUserTestsAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ForInvalidTestId_ReturnsNull()
        {
            // Assert
            const int testId = 50;
            const int userId = 1;

            // Act
            var test = await _testService.GetByIdAsync(testId, userId);

            // Assert
            Assert.Null(test);
        }

        [Fact]
        public async Task GetByIdAsync_ForValidTestIdButUserIdIsNotOwningTest_ReturnsNull()
        {
            // Arrange
            const int testId = 1;
            const int userId = 1;

            // Act
            var test = await _testService.GetByIdAsync(testId, userId);

            // Assert
            Assert.Null(test);
        }

        [Fact]
        public async Task GetByIdAsync_ForValidTestId_ReturnsTestWithQuestionsAndAnswers()
        {
            // Arrange
            const int testId = 1;
            const int userId = 5;

            // Act
            var test = await _testService.GetByIdAsync(testId, userId);
            var questions = test?.Questions;

            // Assert
            Assert.NotNull(test);
            Assert.Equal(2, questions?.Count);
            Assert.Equal("Question 1 test 1", questions?[0].Question);
        }

        [Fact]
        public async Task CreateNewTestAsync_ForEmptyTitle_ReturnsResultObjectWithErrorMessage()
        {
            // Arrange
            var test = new CreateTestDto
            {
                Description = "Description"
            };

            // Act
            var result = await _testService.CreateNewTestAsync(test, 5);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.EmptyTestTitle, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewTestAsync_ForValidParameters_ReturnsSuccessAndCreateTest()
        {
            // Arrange
            const string title = "New title";

            var test = new CreateTestDto
            {
                Title = title,
                Description = "Description"
            };

            // Act
            var result = await _testService.CreateNewTestAsync(test, 5);
            var foundTest = await _testService.GetByIdAsync(result.Value, 5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(title, foundTest?.Title);
        }

        [Fact]
        public async Task CreateNewTestWithQuestionsAsync_ForEmptyTitle_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int userId = 5;
            var testDto = new CreateTestDto
            {
                Description = "New test description",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<CreateAnswerDto>
                        {
                            new CreateAnswerDto { Answer = "A", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
            };

            // Act
            var result = await _testService.CreateNewTestWithQuestionsAsync(testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.ValidationError, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewTestWithQuestionsAsync_ForQuestionWithoutCorrectAnswer_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int userId = 5;
            var testDto = new CreateTestDto
            {
                Title = "New test",
                Description = "New test description",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<CreateAnswerDto>
                        {
                            new CreateAnswerDto { Answer = "A", Correct = false },
                            new CreateAnswerDto { Answer = "B", Correct = false }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
            };

            // Act
            var result = await _testService.CreateNewTestWithQuestionsAsync(testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.ValidationError, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewTestWithQuestionsAsync_ForQuestionWithSingleChoiceOptionsWithMoreThanOneCorrectAnswer_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int userId = 5;
            var testDto = new CreateTestDto
            {
                Title = "New test",
                Description = "New test description",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<CreateAnswerDto>
                        {
                            new CreateAnswerDto { Answer = "A", Correct = true },
                            new CreateAnswerDto { Answer = "B", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
            };

            // Act
            var result = await _testService.CreateNewTestWithQuestionsAsync(testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.ValidationError, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewTestWithQuestionsAsync_ForQuestionWithMultipleChoiceOptionsWithMoreThanOneCorrectAnswer_ReturnsSuccessTrueWithTestDtoObject()
        {
            // Arrange
            const int userId = 5;
            var testDto = new CreateTestDto
            {
                Title = "New test",
                Description = "New test description",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.MultipleChoice,
                        Answers = new List<CreateAnswerDto>
                        {
                            new CreateAnswerDto { Answer = "A", Correct = true },
                            new CreateAnswerDto { Answer = "B", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
            };

            // Act
            var result = await _testService.CreateNewTestWithQuestionsAsync(testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(testDto.Title, result.Value.Title);
            Assert.Equal(testDto.Questions.Count, result.Value.Questions?.Count);
        }

        [Fact]
        public async Task CreateNewTestWithQuestionsAsync_ForValidParameters_ReturnsSuccessTrueWithTestDtoObject()
        {
            // Arrange
            const int userId = 5;
            var testDto = new CreateTestDto
            {
                Title = "New test",
                Description = "New test description",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<CreateAnswerDto>
                        {
                            new CreateAnswerDto { Answer = "A", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
            };

            // Act
            var result = await _testService.CreateNewTestWithQuestionsAsync(testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(testDto.Title, result.Value.Title);
            Assert.Equal(testDto.Questions.Count, result.Value.Questions?.Count);
        }
        
        [Fact]
        public async Task ModifyTestAsync_ForUserWhoIsNotOwningTest_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int testId = 2;
            const int userId = 7;
            var testDto = new TestDto
            {
                Description = "New test 2 description",
                Questions = new List<QuestionDto>
                {
                    new QuestionDto
                    {
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<AnswerDto>
                        {
                            new AnswerDto { Answer = "A", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
                IsPublic = true,
            };

            // Act
            var result = await _testService.ModifyTestAsync(testId, testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(GeneralErrorMessages.Unauthorized, result.ErrorMessage);
        }

        [Fact]
        public async Task ModifyTestAsync_ForNotValidQuestion_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int testId = 2;
            const int userId = 5;
            var testDto = new TestDto
            {
                Description = "New test 2 description",
                Questions = new List<QuestionDto>
                {
                    new QuestionDto
                    {
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<AnswerDto>
                        {
                            new AnswerDto { Answer = "A", Correct = false },
                            new AnswerDto { Answer = "B", Correct = false }
                        },
                        MathMode = false
                    }
                },
                IsPublic = false,
            };

            // Act
            var result = await _testService.ModifyTestAsync(testId, testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(TestErrorMessages.ValidationError, result.ErrorMessage);
        }

        [Fact]
        public async Task ModifyTestAsync_ForIncorrectParameters_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int testId = 2;
            const int userId = 5;
            var testDto = new TestDto
            {
                Description = "New test 2 description",
                Questions = new List<QuestionDto>
                {
                    new QuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<AnswerDto>
                        {
                            new AnswerDto { Answer = "A", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
                IsPublic = true,
            };

            // Act
            var result = await _testService.ModifyTestAsync(testId, testDto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(TestErrorMessages.ValidationError, result.ErrorMessage);
        }

        [Fact]
        public async Task ModifyTestAsync_ForCorrectParameters_ModifiesTestAndReturnsResultObject()
        {
            // Arrange
            const int testId = 2;
            const int userId = 5;
            var testDto = new TestDto
            {
                Title = "New test 2 title",
                Description = "New test 2 description",
                Questions = new List<QuestionDto>
                {
                    new QuestionDto
                    {
                        Question = "Question content",
                        QuestionType = QuestionType.SingleChoice,
                        Answers = new List<AnswerDto>
                        {
                            new AnswerDto { Answer = "A", Correct = true }
                        },
                        MathMode = false
                    }
                },
                ShuffleAnswers = true,
                IsPublic = true,
            };

            // Act
            var result = await _testService.ModifyTestAsync(testId, testDto, userId);
            var searchResult = await _testService.GetByIdAsync(testId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.NotNull(searchResult);
            Assert.Equal(testDto.Title, searchResult.Title);
            Assert.Equal(testDto.Description, searchResult.Description);
            Assert.Equal(testDto.ShuffleAnswers, searchResult.ShuffleAnswers);
        }

        [Fact]
        public async Task DuplicateTestAsync_ForTestWhichNotExists_ReturnsSuccessFalseWithProperErrorMessage()
        {
            // Arrange
            const int testId = 123;
            const int userId = 5;

            // Act
            var result = await _testService.DuplicateTestAsync(testId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.NotFound, result.ErrorMessage);
        }

        [Fact]
        public async Task DuplicateTestAsync_ForGivenTestIdAndUserId_CreatesCopyOfTestAndReturnsResultObjectWithNewTestId()
        {
            // Arrange
            const int testId = 1;
            const int userId = 5;

            // Act
            var result = await _testService.DuplicateTestAsync(testId, userId);
            var searchOriginalTestResult = await _testService.GetByIdAsync(testId, userId);
            var searchDuplicatedTestResult = await _testService.GetByIdAsync(result.Value, userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(string.Concat(searchOriginalTestResult?.Title, " - Copy"), searchDuplicatedTestResult?.Title);
            Assert.Equal(searchOriginalTestResult?.Description, searchDuplicatedTestResult?.Description);
            Assert.Equal(searchOriginalTestResult?.Questions?.Count, searchDuplicatedTestResult?.Questions?.Count);
        }

        [Fact]
        public async Task GetAllPublicTestsListAsync_ReturnsAllTestWhichHasIsPublicSetToTrue()
        {
            // Act
            var result = await _testService.GetAllPublicTestsListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test 1", result[0].Title);
        }
        
        [Fact]
        public async Task GetAllUserTestsWithQuestionsContentAsync_ForGivenUserId_ReturnsListOfTestDtoObjects()
        {
            // Arrange
            var userId = 5;

            // Act
            var result = await _testService.GetAllUserTestsWithQuestionsContentAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Test 1", result[0].Title);
            Assert.Equal("Question 1 test 1", result[0].Questions?[0].Question);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 2)]
        [InlineData(10, 10)]
        public async Task AddQuestionToTestAsync_ForInvalidQuestionAndSetId_ReturnsFalse(int setId, int questionId)
        {
            // Act
            var result = await _testService.AddQuestionToTestAsync(setId, questionId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddQuestionToTestAsync_ForValidIds_ReturnsTrueAndAddQuestionToSet()
        {
            // Arrange
            const int testId = 2;
            const int userId = 5;
            const int questionId = 2;

            // Act
            var result = await _testService.AddQuestionToTestAsync(testId, questionId);
            var questionCount = await _testService.GetByIdAsync(testId, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(1, questionCount?.Questions?.Count);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 2)]
        [InlineData(10, 10)]
        public async Task RemoveQuestionFromSetAsync_ForInvalidIds_ReturnsFalse(int testId, int questionId)
        {
            // Act
            var result = await _testService.RemoveQuestionFromTestAsync(testId, questionId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveQuestionFromSetAsync_ForValidIds_ReturnsTrueAndAddRemoveQuestionFromSet()
        {
            // Arrange
            const int testId = 1;
            const int questionId = 1;
            const int userId = 5;

            // Act
            var result = await _testService.RemoveQuestionFromTestAsync(testId, questionId);
            var questionCount = await _testService.GetByIdAsync(testId, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(1, questionCount?.Questions?.Count);
        }

        [Fact]
        public async Task DeleteByIdAsync_ForInvalidId_ReturnsFalse()
        {
            // Arrange
            const int testId = -2;
            const int userId = 5;

            // Act
            var beforeDeleting = await _testService.GetAllUserTestsAsync(userId);
            var isDeleted = await _testService.DeleteByIdAsync(testId);
            var afterDeleting = await _testService.GetAllUserTestsAsync(userId);

            // Assert
            Assert.NotNull(beforeDeleting);
            Assert.NotNull(afterDeleting);
            Assert.False(isDeleted);
            Assert.Equal(afterDeleting.Count, beforeDeleting.Count);
        }

        [Fact]
        public async Task DeleteByIdAsync_ForValid_ReturnsTrueAndChangeQuestionFlag()
        {
            // Arrange
            const int testId = 2;
            const int userId = 5;

            // Act
            var beforeDeleting = await _testService.GetAllUserTestsAsync(userId);
            var isDeleted = await _testService.DeleteByIdAsync(testId);
            var afterDeleting = await _testService.GetAllUserTestsAsync(userId);

            // Assert
            Assert.NotNull(beforeDeleting);
            Assert.NotNull(afterDeleting);
            Assert.True(isDeleted);
            Assert.Equal(afterDeleting.Count, beforeDeleting.Count - 1);
        }

        private static Mock<ITestRepository> GetSetRepositoryMock(List<Test> tests)
        {
            var mock = new Mock<ITestRepository>();
            mock.Setup(x => x.GetTestWithQuestionsByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool _, bool _) => tests.FirstOrDefault(e => e.Id == id));

            mock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool _) => tests.FirstOrDefault(e => e.Id == id));

            mock.Setup(x => x.GetTestsByUserIdAsync(It.IsAny<int?>(), It.IsAny<bool>())).ReturnsAsync((int? userId, bool _) =>
            {
                return userId is null ? tests : tests.Where(e => e.UserId == userId && !e.IsDeleted).ToList();
            });

            mock.Setup(x => x.GetPublicTestsListAsync()).ReturnsAsync(tests.Where(e => e.IsPublic).ToList());

            mock.Setup(x => x.AddAsync(It.IsAny<Test>()))
                .Callback((Test test) =>
                {
                    test.Id = 30;
                    tests.Add(test);
                });

            mock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            return mock;
        }

        private static List<Question> GetQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    Id = 1,
                    Content = "Question 1 set 1",
                    QuestionType = QuestionType.ShortAnswer,
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer
                        {
                            Content = "Answer a",
                            Correct = true
                        },
                        new QuestionAnswer
                        {
                            Content = "Answer b",
                            Correct = false
                        },
                        new QuestionAnswer
                        {
                            Content = "Answer c",
                            Correct = false
                        }
                    },
                    IsDeleted = false
                },
                new Question
                {
                    Id = 2,
                    Content = "Question 2",
                    QuestionType = QuestionType.ShortAnswer,
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer
                        {
                            Content = "Answer a",
                            Correct = true
                        },
                        new QuestionAnswer
                        {
                            Content = "Answer b",
                            Correct = false
                        },
                    },
                    IsDeleted = false
                },
                new Question
                {
                    Id = 3,
                    Content = "Question 2 set 1",
                    QuestionType = QuestionType.TrueFalse,
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer
                        {
                            Content = "Answer true",
                            Correct = true
                        },
                        new QuestionAnswer
                        {
                            Content = "Answer false",
                            Correct = false
                        },
                    },
                    IsDeleted = false
                }
            };
        }


        private static List<Test> GetTests()
        {
            return new List<Test>
            {
                new Test
                {
                    Id = 1,
                    Title = "Test 1",
                    Description = "Description 1",
                    IsDeleted = false,
                    UserId = 5,
                    IsPublic = true,
                    User = new User
                    {
                        FirstName = "Adam",
                        LastName = "Abacki",
                        UserName = "AdamAbacki",
                        Email = "a@a.pl",
                        IsDeleted = false,
                        AccountConfirmed = true,
                        Role = new Role { Id = 2, Name = "User" }
                    },
                    Questions = new List<Question>
                    {
                        new Question
                            {
                                Id = 1,
                                Content = "Question 1 test 1",
                                QuestionType = QuestionType.ShortAnswer,
                                Answers = new List<QuestionAnswer> {
                                    new QuestionAnswer
                                    {
                                        Content = "Answer a",
                                        Correct = true
                                    },
                                    new QuestionAnswer
                                    {
                                        Content = "Answer b",
                                        Correct = false
                                    },
                                    new QuestionAnswer
                                    {
                                        Content = "Answer c",
                                        Correct = false
                                    }
                                },
                                IsDeleted = false
                            },
                        new Question
                            {
                                Id = 3,
                                Content = "Question 2 test 1",
                                QuestionType = QuestionType.TrueFalse,
                                Answers = new List<QuestionAnswer> {
                                    new QuestionAnswer
                                    {
                                        Content = "Answer true",
                                        Correct = true
                                    },
                                    new QuestionAnswer
                                    {
                                        Content = "Answer false",
                                        Correct = false
                                    },
                                },
                                IsDeleted = false
                            }
                        },
                },
                new Test
                {
                    Id = 2,
                    Title = "Test 2",
                    Description = "Description 2",
                    IsDeleted = false,
                    UserId = 5,
                    User = new User
                    {
                        FirstName = "Adam",
                        LastName = "Abacki",
                        UserName = "AdamAbacki",
                        Email = "a@a.pl",
                        IsDeleted = false,
                        AccountConfirmed = true,
                        Role = new Role { Id = 2, Name = "User" }
                    },
                    Questions = new List<Question>()
                },
                new Test
                {
                    Id = 3,
                    Title = "Test 3",
                    Description = "Description 3",
                    IsDeleted = false,
                    UserId = 1,
                    User = new User
                    {
                        FirstName = "Admin",
                        LastName = "Abacki",
                        UserName = "Admin",
                        Email = "admin@admin.pl",
                        IsDeleted = false,
                        AccountConfirmed = true,
                        Role = new Role { Id = 1, Name = "Admin" }
                    },
                    Questions = new List<Question>()
                },
            };
        }
    }
}
