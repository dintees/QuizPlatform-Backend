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

        // TODO AddQuestionToSetAsync

        // TODO RemoveQuestionFromSetAsync

        // TODO DeleteByIdAsync


        private Mock<ISetRepository> GetSetRepositoryMock(List<Set> sets)
        {
            var mock = new Mock<ISetRepository>();
            mock.Setup(x => x.GetSetWithQuestionsByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync((int id, bool readOnly) => sets.FirstOrDefault(e => e.Id == id));
            mock.Setup(x => x.InsertSetAsync(It.IsAny<Set>()))
                .Callback((Set set) =>
                {
                    set.Id = 3;
                    sets.Add(set);
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
