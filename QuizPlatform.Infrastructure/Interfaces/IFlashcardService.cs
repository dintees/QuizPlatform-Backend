using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Flashcard;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IFlashcardService
{
    Task<Result<List<UserFlashcardDto>>> GetUserFlashcardsListAsync(int userId);
    Task<FlashcardsSetDto?> GetFlashcardItemsByIdAsync(int flashcardsId);
    Task<int?> CreateNewFlashcardsSetAsync(FlashcardsSetDto dto, int userId);
    Task<bool> ModifyFlashcardsSetAsync(FlashcardsSetDto dto, int id, int userId);
    Task DeleteFlashcardsSetByIdAsync(int flashcardsSetId);
    Task<int?> GenerateFlashcardsSetFromTestAsync(int testId, int userId);
}