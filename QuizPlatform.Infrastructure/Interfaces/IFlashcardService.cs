using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Flashcard;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IFlashcardService
{
    Task<Result<List<UserFlashcardDto>>> GetUserFlashcardsListAsync(int userId);
    Task<List<FlashcardItemDto>?> GetFlashcardItemsById(int flashcardsId);
}