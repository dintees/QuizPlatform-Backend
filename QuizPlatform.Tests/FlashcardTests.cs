using AutoMapper;
using Moq;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Flashcard;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class FlashcardTests
    {
        private readonly IFlashcardService _flashcardService;

        public FlashcardTests()
        {
            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            var flashcards = GetFlashcards();
            var flashcardRepositoryMock = GetFlashcardRepositoryMock(flashcards);

            var testRepositoryMock = new Mock<ITestRepository>();
            testRepositoryMock.Setup(x => x.GetTestWithQuestionsByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync((int _, bool _, bool _) => new Test()
            {
                Id = 1,
                Title = "My test",
                Description = "My test description",
                IsDeleted = false,
                IsPublic = false,
                OneQuestionMode = false,
                ShuffleAnswers = false,
                ShuffleQuestions = false,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Id = 1,
                        Content = "Pies",
                        Answers = new List<QuestionAnswer>
                        {
                            new QuestionAnswer { Id = 1, Content = "Dog", Correct = true }
                        },
                        QuestionType = QuestionType.ShortAnswer
                    },
                    new Question
                    {
                        Id = 2,
                        Content = "Kot",
                        Answers = new List<QuestionAnswer>
                        {
                            new QuestionAnswer { Id = 2, Content = "Cat", Correct = true },
                            new QuestionAnswer { Id = 3, Content = "Hedgehog", Correct = false }
                        },
                        QuestionType = QuestionType.SingleChoice
                    }
                },
                TsInsert = DateTime.Now,
                TsUpdate = DateTime.Now
            });

            _flashcardService = new FlashcardService(flashcardRepositoryMock.Object, testRepositoryMock.Object, mapper);
        }

        [Fact]
        public async Task GetUserFlashcardsListAsync_ForUserId_ReturnsResultObject()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _flashcardService.GetUserFlashcardsListAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Value?.Count);
        }

        [Fact]
        public async Task GetFlashcardItemsByIdAsync_ForInvalidFlashcardsSetId_ReturnsNull()
        {
            // Arrange
            var flashcardsSetId = 100;

            // Act
            var result = await _flashcardService.GetFlashcardItemsByIdAsync(flashcardsSetId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFlashcardItemsByIdAsync_ForValidFlashcardsSetId_ReturnsFlashcardsSetDto()
        {
            // Arrange
            var flashcardsSetId = 1;

            // Act
            var result = await _flashcardService.GetFlashcardItemsByIdAsync(flashcardsSetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Flashcards set 1", result.Title);
            Assert.Equal("First", result.Description);
            Assert.Equal(3, result.FlashcardItems?.Count);
        }

        [Fact]
        public async Task CreateNewFlashcardsSetAsync_ForGivenObject_CreatesFlashcardsSetAndReturnsId()
        {
            // Arrange
            var flashcardsSet = new FlashcardsSetDto
            {
                Title = "Flashcards new set",
                Description = "Created",
                FlashcardItems = new List<FlashcardItemDto>
                {
                    new FlashcardItemDto { FirstSide = "1.1st", SecondSide = "1.2st" },
                    new FlashcardItemDto { FirstSide = "2.1st", SecondSide = "2.2st" },
                }
            };

            // Act
            var id = await _flashcardService.CreateNewFlashcardsSetAsync(flashcardsSet, 1);
            var result = await _flashcardService.GetFlashcardItemsByIdAsync(id ?? 0);

            // Assert
            Assert.NotNull(id);
            Assert.Equal(8, id);
            Assert.NotNull(result);
            Assert.Equal("Flashcards new set", result.Title);
            Assert.Equal(2, result.FlashcardItems?.Count);
        }

        [Fact]
        public async Task ModifyFlashcardsSetAsync_ForInvalidFlashcardsId_ReturnsFalse()
        {
            // Arrange
            var flashcardsSetDto = new FlashcardsSetDto
            {
                Title = "Flashcards set 3 modified",
                Description = "Third modified",
                FlashcardItems = new List<FlashcardItemDto>
                {
                    new FlashcardItemDto { Id = 7, FirstSide = "1.1st edited", SecondSide = "1.2st edited", },
                    new FlashcardItemDto { Id = 8, FirstSide = "2.1st edited", SecondSide = "2.2st edited", },
                }
            };

            // Act
            var result = await _flashcardService.ModifyFlashcardsSetAsync(flashcardsSetDto, 10, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ModifyFlashcardsSetAsync_ForValidData_ModifiesFlashcardsSetAndReturnsTrue()
        {
            // Arrange
            var flashcardsSetDto = new FlashcardsSetDto
            {
                Title = "Flashcards set 3 modified",
                Description = "Third modified",
                FlashcardItems = new List<FlashcardItemDto>
                {
                    new FlashcardItemDto { Id = 7, FirstSide = "1.1st edited", SecondSide = "1.2st edited", },
                    new FlashcardItemDto { Id = 8, FirstSide = "2.1st edited", SecondSide = "2.2st edited", },
                }
            };

            // Act
            var result = await _flashcardService.ModifyFlashcardsSetAsync(flashcardsSetDto, 3, 1);
            var searchResult = await _flashcardService.GetFlashcardItemsByIdAsync(3);

            // Assert
            Assert.True(result);
            Assert.NotNull(searchResult);
            Assert.Equal(flashcardsSetDto.Title, searchResult.Title);
            Assert.Equal(flashcardsSetDto.Description, searchResult.Description);
            Assert.Equal(flashcardsSetDto.FlashcardItems[0].FirstSide, searchResult.FlashcardItems?[0].FirstSide);
        }

        [Fact]
        public async Task DeleteFlashcardsSetByIdAsync_DeleteFlashcardsSet()
        {
            // Arrange
            var flashcardsSetId = 2;

            // Act
            await _flashcardService.DeleteFlashcardsSetByIdAsync(flashcardsSetId);
            var searchResult = await _flashcardService.GetFlashcardItemsByIdAsync(flashcardsSetId);

            // Assert
            Assert.Null(searchResult);
        }

        [Fact]
        public async Task GenerateFlashcardsSetFromTestAsync_ForGiverTestId_ReturnsIdOfNewFlashcardsCreatedSet()
        {
            // Arrange
            var testId = 1;
            var userId = 1;

            // Act
            var result = await _flashcardService.GenerateFlashcardsSetFromTestAsync(testId, userId);
            var searchResult = await _flashcardService.GetFlashcardItemsByIdAsync(result ?? 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.Value);
            Assert.NotNull(searchResult);
            Assert.Equal("My test - Flashcards", searchResult.Title);
            Assert.Equal(2, searchResult.FlashcardItems?.Count);
            Assert.Equal("Kot", searchResult.FlashcardItems?[1].FirstSide);
            Assert.Equal("Cat", searchResult.FlashcardItems?[1].SecondSide);
        }


        private Mock<IFlashcardRepository> GetFlashcardRepositoryMock(List<Flashcard> flashcards)
        {
            var flashcardRepositoryMock = new Mock<IFlashcardRepository>();
            flashcardRepositoryMock.Setup(x => x.GetFlashcardByUserId(It.IsAny<int>())).ReturnsAsync((int userId) => flashcards.Where(e => e.UserId == userId).ToList());
            flashcardRepositoryMock.Setup(x => x.GetFlashcardsSetByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => flashcards.FirstOrDefault(e => e.Id == id));
            flashcardRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Flashcard>())).Returns((Flashcard flashcard) => { flashcard.Id = 8; flashcards.Add(flashcard); return Task.FromResult(8); });
            flashcardRepositoryMock.Setup(x => x.Delete(It.IsAny<Flashcard>())).Callback((Flashcard flashcard) => { flashcards.Remove(flashcards.First(e => e.Id == flashcard.Id)); });
            flashcardRepositoryMock.Setup(x => x.SaveAsync()).ReturnsAsync(true);
            return flashcardRepositoryMock;
        }

        private List<Flashcard> GetFlashcards()
        {
            var flashcards = new List<Flashcard>
            {
                new Flashcard
                {
                    Id = 1,
                    Title = "Flashcards set 1",
                    Description = "First",
                    UserId = 1,
                    TsInsert = DateTime.Now,
                    TsUpdate = DateTime.Now,
                    FlashcardItems = new List<FlashcardItem>
                    {
                        new FlashcardItem { Id = 1, FirstSide = "1.1st", SecondSide = "1.2st", FlashcardId = 1 },
                        new FlashcardItem { Id = 2, FirstSide = "2.1st", SecondSide = "2.2st", FlashcardId = 1 },
                        new FlashcardItem { Id = 3, FirstSide = "3.1st", SecondSide = "3.2st", FlashcardId = 1 },
                    }
                },
                new Flashcard
                {
                    Id = 2,
                    Title = "Flashcards set 2",
                    Description = "Second",
                    UserId = 2,
                    TsInsert = DateTime.Now,
                    TsUpdate = DateTime.Now,
                    FlashcardItems = new List<FlashcardItem>
                    {
                        new FlashcardItem { Id = 4, FirstSide = "1.1st", SecondSide = "1.2st", FlashcardId = 2 },
                        new FlashcardItem { Id = 5, FirstSide = "2.1st", SecondSide = "2.2st", FlashcardId = 2 },
                        new FlashcardItem { Id = 6, FirstSide = "3.1st", SecondSide = "3.2st", FlashcardId = 2 },
                    }
                },
                new Flashcard
                {
                    Id = 3,
                    Title = "Flashcards set 3",
                    Description = "Third",
                    UserId = 1,
                    TsInsert = DateTime.Now,
                    TsUpdate = DateTime.Now,
                    FlashcardItems = new List<FlashcardItem>
                    {
                        new FlashcardItem { Id = 7, FirstSide = "1.1st", SecondSide = "1.2st", FlashcardId = 3 },
                        new FlashcardItem { Id = 8, FirstSide = "2.1st", SecondSide = "2.2st", FlashcardId = 3 },
                    }
                }
            };
            return flashcards;
        }

    }
}
