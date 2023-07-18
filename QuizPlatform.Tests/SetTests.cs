using AutoMapper;
using FluentValidation;
using Moq;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Set;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class SetTests
    {
        private readonly SetService _service;

        public SetTests()
        {
            var sets = GetSets();
            var setRepositoryMock = GetSetRepositoryMock(sets);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            var questionRepositoryMock = new Mock<IQuestionRepository>();
            questionRepositoryMock.Setup(x => x.GetQuestionByIdAsync(2, It.IsAny<bool>())).ReturnsAsync(new Question
            {
                Id = 2,
                Content = "Sample question",
                QuestionType = QuestionTypeName.ShortAnswer,
                IsDeleted = false,
                Answers = new List<QuestionAnswer> { new QuestionAnswer { Content = "a", Correct = true } }
            });

            IValidator<Set> setValidator = new SetValidator();

            _service = new SetService(mapper, setRepositoryMock.Object, questionRepositoryMock.Object, setValidator);
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
            Assert.Equal("Question 1 set 1", questions![0].Question);
        }

        [Fact]
        public async Task CreateNewSetAsync_ForEmptyTitle_ReturnsResultObjectWithErrorMessage()
        {
            var set = new CreateSetDto
            {
                Description = "Description"
            };

            var result = await _service.CreateNewSetAsync(set);

            Assert.False(result.Success);
            Assert.Equal(SetErrorMessages.EmptySetTitle, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewSetAsync_ForValidParameters_ReturnsSuccessAndCreateSet()
        {
            const string title = "New title";

            var set = new CreateSetDto
            {
                Title = title,
                Description = "Description"
            };

            var result = await _service.CreateNewSetAsync(set);
            var foundSet = await _service.GetByIdAsync(3);

            Assert.True(result.Success);
            Assert.Equal(title, foundSet?.Title);
        }

        [Fact]
        public async Task ModifySetPropertiesAsync_ForEmptyTitleAttribute_ReturnsErrorMessage()
        {
            var setOptions = new SetDto { Description = "New description" };

            var result = await _service.ModifySetPropertiesAsync(2, setOptions);
            var foundSet = await _service.GetByIdAsync(2);

            Assert.Equal(SetErrorMessages.EmptySetTitle, result);
            Assert.Equal("Set 2", foundSet?.Title);
        }

        [Fact]
        public async Task ModifySetPropertiesAsync_ForCorrectSetObject_ReturnsNull()
        {
            var setOptions = new SetDto { Title = "New title", Description = "New description" };

            var result = await _service.ModifySetPropertiesAsync(2, setOptions);
            var foundSet = await _service.GetByIdAsync(2);

            Assert.Null(result);
            Assert.Equal("New title", foundSet?.Title);
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
        public async Task RemoveQuestionToSetAsync_ForValidIds_ReturnsTrueAndAddRemoveQuestionFromSet()
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


        private Mock<ISetRepository> GetSetRepositoryMock(List<Set> sets)
        {
            var mock = new Mock<ISetRepository>();
            mock.Setup(x => x.GetSetWithQuestionsByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool readOnly) => sets.FirstOrDefault(e => e.Id == id));
            mock.Setup(x => x.GetSetByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool readOnly) => sets.FirstOrDefault(e => e.Id == id));
            mock.Setup(x => x.InsertSetAsync(It.IsAny<Set>()))
                .Callback((Set set) =>
                {
                    set.Id = 3;
                    sets.Add(set);
                });
            mock.Setup(x => x.InsertQuestionSetAsync(It.IsAny<QuestionSet>())).Callback((QuestionSet questionSet) =>
            {
                var set = sets.FirstOrDefault(e => e.Id == questionSet?.Set?.Id);
                if (set?.Questions is null) set!.Questions = new List<QuestionSet>();
                set?.Questions?.Add(questionSet);
            });
            mock.Setup(x => x.GetQuestionSetBySetIdAndQuestionIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((int setId, int questionId) =>
                {
                    var set = sets.FirstOrDefault(e => e.Id == setId);
                    if (set is null) return null;
                    var question = set?.Questions!.FirstOrDefault(e => e.Question!.Id == questionId);
                    if (question is null || question!.Question is null) return null;
                    return new QuestionSet { QuestionId = questionId, SetId = setId };
                });
            mock.Setup(x => x.RemoveQuestionFromSet(It.IsAny<QuestionSet>())).Callback((QuestionSet questionSet) =>
            {
                var set = sets.FirstOrDefault(e => e.Id == questionSet?.SetId);
                var qq = set?.Questions!.FirstOrDefault(e => e.Question!.Id == questionSet.QuestionId);
                set!.Questions?.Remove(qq!);

            });
            mock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            return mock;
        }

        private List<Set> GetSets()
        {
            return new List<Set>
            {
                new Set
                {
                    Id = 1,
                    Title = "Set 1",
                    Description = "Description 1",
                    IsDeleted = false,
                    Questions = new List<QuestionSet>
                    {
                        new QuestionSet
                        {
                            Question = new Question
                            {
                                Id = 1,
                                Content = "Question 1 set 1",
                                QuestionType = QuestionTypeName.ShortAnswer,
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
                            }
                        },
                        new QuestionSet
                        {
                            Question = new Question
                            {
                                Id = 3,
                                Content = "Question 2 set 1",
                                QuestionType = QuestionTypeName.TrueFalse,
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
                    }
                },
                new Set
                {
                    Id = 2,
                    Title = "Set 2",
                    Description = "Description 2",
                    IsDeleted = false,
                    Questions = null
                }
            };
        }
    }
}
