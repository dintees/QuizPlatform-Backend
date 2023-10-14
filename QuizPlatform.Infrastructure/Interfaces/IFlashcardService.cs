using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Flashcard;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IFlashcardService
{
    Task<Result<List<UserFlashcardDto>>> GetUserFlashcardsListAsync(int userId);
    Task<FlashcardsSetDto?> GetFlashcardItemsById(int flashcardsId);
    Task<int?> CreateNewFlashcardsSet(FlashcardsSetDto dto, int userId);
    Task<bool> ModifyFlashcardsSet(FlashcardsSetDto dto, int id, int userId);
    Task DeleteFlashcardsSetById(int flashcardsSetId);
    Task<int?> GenerateFlashcardsSetFromTest(int testId, int userId);
}