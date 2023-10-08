using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IFlashcardRepository
{
    Task<List<Flashcard>?> GetFlashcardByUserId(int userId);
    Task<List<FlashcardItem>?> GetFlashcardItemsById(int flashcardsId);
}