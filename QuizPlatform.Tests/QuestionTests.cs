using AutoMapper;
using FluentValidation;
using Moq;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class QuestionTests
    {
        private readonly QuestionService _service;

        public QuestionTests()
        {
            var questions = GetQuestions();
            var questionServiceMock = GetQuestionRepositoryMock(questions);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            IValidator<Question> questionValidator = new QuestionValidator();

            _service = new QuestionService(questionServiceMock.Object, mapper, questionValidator);
        }

        [Fact]
        public async Task GetByIdAsync_ForInvalidQuestionId_ReturnsNull()
        {
            var question = await _service.GetByIdAsync(10);

            Assert.Null(question);
        }

        [Fact]
        public async Task GetByIdAsync_ForValidQuestionId_ReturnsQuestion()
        {
            var question = await _service.GetByIdAsync(2);

            Assert.IsType<QuestionDto>(question);
            Assert.Equal("Question 2", question!.Question);
            Assert.Equal(3, question!.Answers?.Count);
            Assert.Equal(QuestionTypeName.MultipleChoice, question.QuestionType);
        }

        [Fact]
        public async Task CreateQuestionAsync_ForEmptyQuestionContent_ReturnsEmptyQuestionContentError()
        {
            var question = new CreateQuestionDto
            {
                Question = null,
                QuestionType = QuestionTypeName.SingleChoice,
                Answers = new List<CreateAnswerDto> { new CreateAnswerDto { Answer = "a", Correct = true} }
            };

            var result = await _service.CreateQuestionAsync(question);

            Assert.False(result.Success);
            Assert.Equal(QuestionErrorMessages.EmptyQuestionContent, result.ErrorMessage!);
        }

        [Fact]
        public async Task CreateQuestionAsync_ForEmptyAnswers_ReturnsEmptyAnswerContentError()
        {
            var question = new CreateQuestionDto
            {
                Question = "Question",
                QuestionType = QuestionTypeName.SingleChoice,
                Answers = null
            };

            var result = await _service.CreateQuestionAsync(question);

            Assert.False(result.Success);
            Assert.Equal(QuestionErrorMessages.EmptyAnswersContent, result.ErrorMessage!);
        }

        [Fact]
        public async Task CreateQuestionAsync_ForWrongNumberOfAnswersTrueFalse_ReturnsWrongNumberOfCorrectAnswersError()
        {
            var question = new CreateQuestionDto
            {
                Question = "Question",
                QuestionType = QuestionTypeName.TrueFalse,
                Answers = new List<CreateAnswerDto>
                {
                    new CreateAnswerDto { Answer = "A1", Correct = true },
                    new CreateAnswerDto { Answer = "A2", Correct = false },
                    new CreateAnswerDto { Answer = "A3", Correct = true },
                }
            };

            var result = await _service.CreateQuestionAsync(question);

            Assert.False(result.Success);
            Assert.Equal(QuestionErrorMessages.WrongNumberOfCorrectAnswers, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateQuestionAsync_ForMoreThanOneCorrectAnswerInSingleChoiceType_ReturnsOneAnswerShouldBeCorrectError()
        {
            var question = new CreateQuestionDto
            {
                Question = "Question",
                QuestionType = QuestionTypeName.SingleChoice,
                Answers = new List<CreateAnswerDto>
                {
                    new CreateAnswerDto { Answer = "A1", Correct = true },
                    new CreateAnswerDto { Answer = "A2", Correct = false },
                    new CreateAnswerDto { Answer = "A3", Correct = true },
                }
            };

            var result = await _service.CreateQuestionAsync(question);

            Assert.False(result.Success);
            Assert.Equal(QuestionErrorMessages.OneAnswerShouldBeCorrect, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateQuestionAsync_ForAnswersInShortAnswerQuestionType_ReturnsWrongNumberOfAnswersError()
        
        {
            var question = new CreateQuestionDto
            {
                Question = "Question",
                QuestionType = QuestionTypeName.ShortAnswer,
                Answers = new List<CreateAnswerDto> { new CreateAnswerDto { Answer = "test", Correct = false } }
            };

            var result = await _service.CreateQuestionAsync(question);

            Assert.False(result.Success);
            Assert.Equal(QuestionErrorMessages.WrongNumberOfAnswers, result.ErrorMessage!);
        }


        [Fact]
        public async Task CreateQuestionAsync_ForValidQuestionAndAnswers_ReturnsSuccessResultObjectAndAddQuestion()
        {
            var question = new CreateQuestionDto
            {
                Question = "New question",
                QuestionType = QuestionTypeName.SingleChoice,
                Answers = new List<CreateAnswerDto> { new CreateAnswerDto { Answer = "a", Correct = false }, new CreateAnswerDto { Answer = "b", Correct = true } }
            };

            var result = await _service.CreateQuestionAsync(question);
            var foundQuestion = await _service.GetByIdAsync(4);

            Assert.True(result.Success);
            Assert.NotNull(foundQuestion);
            Assert.Equal("New question", foundQuestion.Question);
        }


        [Fact]
        public async Task ModifyQuestionAsync_ForTwoCorrectAnswersInSingleAnswerQuestionMode_ReturnsResultWithProperErrorMessage()
        {
            var question = new CreateQuestionDto
            {
                Question = "Question",
                QuestionType = QuestionTypeName.TrueFalse,
                Answers = new List<CreateAnswerDto>
                {
                    new CreateAnswerDto { Answer = "A1", Correct = true },
                    new CreateAnswerDto { Answer = "A2", Correct = false },
                    new CreateAnswerDto { Answer = "A3", Correct = true },
                }
            };

            var result = await _service.ModifyQuestionAsync(2, question);

            //Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(QuestionErrorMessages.WrongNumberOfCorrectAnswers, result.ErrorMessage);
        }

        [Fact]
        public async Task ModifyQuestionAsync_ForValidQuestionAndAnswers_ReturnsSuccessWithIdAndModifyQuestion()
        {
            var question = new CreateQuestionDto
            {
                Question = "Edited question",
                QuestionType = QuestionTypeName.SingleChoice,
                Answers = new List<CreateAnswerDto> { new CreateAnswerDto { Answer = "a", Correct = false }, new CreateAnswerDto { Answer = "b", Correct = true } }
            };

            var result = await _service.ModifyQuestionAsync(2, question);
            var foundQuestion = await _service.GetByIdAsync(2);

            Assert.True(result.Success);
            Assert.Equal(2, result.Value);
            Assert.NotNull(foundQuestion);
            Assert.Equal("Edited question", foundQuestion.Question);
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



        private Mock<IQuestionRepository> GetQuestionRepositoryMock(List<Question> questions)
        {
            var questionRepositoryMock = new Mock<IQuestionRepository>();
            questionRepositoryMock.Setup(x => x.GetQuestionByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).Returns((int id, bool readOnly) => Task.FromResult(questions.FirstOrDefault(e => e.Id == id)));
            questionRepositoryMock.Setup(x => x.InsertQuestionAsync(It.IsAny<Question>())).Returns((Question question) =>
            {
                question.Id = 4;
                questions.Add(question);
                return Task.FromResult(question.Id);
            });
            questionRepositoryMock.Setup(x => x.UpdateQuestion(It.IsAny<Question>())).Callback((Question question) =>
            {
                var q = questions.FirstOrDefault(q => q.Id == question.Id);
                q = question;
            });
            questionRepositoryMock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            return questionRepositoryMock;
        }

        private List<Question> GetQuestions()
        {
            return new List<Question>
            {
                new Question
                {
                    Id = 1,
                    Content = "Question 1",
                    QuestionType = QuestionTypeName.SingleChoice,
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
                    IsDeleted = false,
                },
                new Question
                {
                    Id = 2,
                    Content = "Question 2",
                    QuestionType = QuestionTypeName.MultipleChoice,
                    Answers = new List<QuestionAnswer> {
                        new QuestionAnswer
                        {
                            Content = "Answer a",
                            Correct = true
                        },
                        new QuestionAnswer
                        {
                            Content = "Answer b",
                            Correct = true
                        },
                        new QuestionAnswer
                        {
                            Content = "Answer c",
                            Correct = false
                        }
                    },
                    IsDeleted = false,
                },
                new Question
                {
                    Id = 3,
                    Content = "Question 3",
                    QuestionType = QuestionTypeName.ShortAnswer,
                    Answers = null,
                    IsDeleted = false,
                }
            };
        }
    }
}
