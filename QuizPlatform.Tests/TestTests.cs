using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class TestTests
    {
        private readonly TestService _service;

        public TestTests()
        {
            var tests = GetTests();
            var setRepositoryMock = GetSetRepositoryMock(tests);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            var questionRepositoryMock = new Mock<IQuestionRepository>();
            questionRepositoryMock.Setup(x => x.GetQuestionByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(
                (int id, bool readOnly) =>
                {
                    return GetQuestions().FirstOrDefault(e => e.Id == id);
                });

            //questionRepositoryMock.Setup(x => x.GetQuestionByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new Question
            //{
            //    Id = 2,
            //    Content = "Sample question",
            //    QuestionType = QuestionTypeName.ShortAnswer,
            //    IsDeleted = false,
            //    Answers = new List<QuestionAnswer> { new QuestionAnswer { Content = "a", Correct = true } }
            //});

            IValidator<Test> setValidator = new TestValidator();

            _service = new TestService(mapper, setRepositoryMock.Object, questionRepositoryMock.Object, setValidator);
        }

        [Fact]
        public async Task GetByIdAsync_ForInvalidSetId_ReturnsNull()
        {
            var set = await _service.GetByIdAsync(20);

            Assert.Null(set);
        }

        [Fact]
        public async Task GetByIdAsync_ForValidSetId_ReturnsSetWithQuestionsAndAnswers()
        {
            var set = await _service.GetByIdAsync(1);
            var questions = set!.Questions;

            Assert.NotNull(set);
            Assert.Equal(2, questions!.Count);
            Assert.Equal("Question 1 set 1", questions[0].Question);
        }

        [Fact]
        public async Task CreateNewSetAsync_ForEmptyTitle_ReturnsResultObjectWithErrorMessage()
        {
            var set = new CreateTestDto
            {
                Description = "Description"
            };

            var result = await _service.CreateNewSetAsync(set, 1);

            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.EmptySetTitle, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewSetAsync_ForValidParameters_ReturnsSuccessAndCreateSet()
        {
            const string title = "New title";

            var set = new CreateTestDto
            {
                Title = title,
                Description = "Description"
            };

            var result = await _service.CreateNewSetAsync(set, 1);
            var foundSet = await _service.GetByIdAsync(3);

            Assert.True(result.Success);
            Assert.Equal(title, foundSet?.Title);
        }

        [Fact]
        public async Task ModifySetPropertiesAsync_ForEmptyTitleAttribute_ReturnsErrorMessage()
        {
            var setOptions = new TestDto { Description = "New description" };

            var result = await _service.ModifySetPropertiesAsync(2, setOptions);
            var foundSet = await _service.GetByIdAsync(2);

            Assert.False(result.Success);
            Assert.Equal(TestErrorMessages.EmptySetTitle, result.ErrorMessage);
            Assert.Equal("Test 2", foundSet?.Title);
        }

        [Fact]
        public async Task ModifySetPropertiesAsync_ForCorrectSetObject_ReturnsSuccessFalseResult()
        {
            var setOptions = new TestDto { Title = "New title", Description = "New description" };

            var result = await _service.ModifySetPropertiesAsync(2, setOptions);
            var foundSet = await _service.GetByIdAsync(2);

            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);
            Assert.Equal(2, result.Value);
            Assert.Equal("New title", foundSet?.Title);
        }

        [Fact]
        public async Task GetAllUserSets_ForGivenUserId_ReturnsSetOwnedByUser()
        {
            // Arrange
            const int userId = 5;

            // Act
            var result = await _service.GetAllUserSets(userId);

            // Assert
            Assert.Equal(2, result?.Count);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 2)]
        [InlineData(10, 10)]
        public async Task AddQuestionToSetAsync_ForInvalidQuestionAndSetId_ReturnsFalse(int setId, int questionId)
        {
            var result = await _service.AddQuestionToSetAsync(setId, questionId);

            Assert.False(result);
        }

        [Fact]
        public async Task AddQuestionToSetAsync_ForValidIds_ReturnsTrueAndAddQuestionToSet()
        {
            var result = await _service.AddQuestionToSetAsync(2, 2);

            var questionCount = await _service.GetByIdAsync(2);

            Assert.True(result);
            Assert.Equal(1, questionCount?.Questions?.Count);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 2)]
        [InlineData(10, 10)]
        public async Task RemoveQuestionFromSetAsync_ForInvalidIds_ReturnsFalse(int setId, int questionId)
        {
            var result = await _service.RemoveQuestionFromSetAsync(setId, questionId);

            Assert.False(result);
        }

        [Fact]
        public async Task RemoveQuestionFromSetAsync_ForValidIds_ReturnsTrueAndAddRemoveQuestionFromSet()
        {
            var result = await _service.RemoveQuestionFromSetAsync(1, 3);

            var questionCount = await _service.GetByIdAsync(1);

            Assert.True(result);
            Assert.Equal(1, questionCount?.Questions?.Count);
        }

        [Fact]
        public async Task DeleteByIdAsync_ForInvalidId_ReturnsFalse()
        {
            var isDeleted = await _service.DeleteByIdAsync(-2);

            Assert.False(isDeleted);
        }

        [Fact]
        public async Task DeleteByIdAsync_ForValid_ReturnsTrueAndChangeQuestionFlag()
        {
            var isDeleted = await _service.DeleteByIdAsync(2);

            Assert.True(isDeleted);
        }

        private Mock<ITestRepository> GetSetRepositoryMock(List<Test> sets)
        {
            var mock = new Mock<ITestRepository>();
            mock.Setup(x => x.GetSetWithQuestionsByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool _) => sets.FirstOrDefault(e => e.Id == id));
            mock.Setup(x => x.GetSetByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool _) => sets.FirstOrDefault(e => e.Id == id));
            mock.Setup(x => x.InsertSetAsync(It.IsAny<Test>()))
                .Callback((Test set) =>
                {
                    set.Id = 3;
                    sets.Add(set);
                });
            mock.Setup(x => x.GetSetsByUserIdAsync(It.IsAny<int>())).ReturnsAsync((int userId) =>
            {
                return sets.Where(e => e.UserId == userId).ToList();
            });
            mock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            return mock;
        }

        private List<Question> GetQuestions()
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


        private List<Test> GetTests()
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
                    Questions = new List<Question>
                    {
                        new Question
                            {
                                Id = 1,
                                Content = "Question 1 set 1",
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
                                Content = "Question 2 set 1",
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
                    Questions = null
                }
            };
    }
}
}
