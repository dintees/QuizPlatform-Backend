using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class FlashcardRepository : IFlashcardRepository
    {
        private readonly ApplicationDbContext _context;

        public FlashcardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Flashcard>?> GetFlashcardByUserId(int userId)
        {
            return await _context.Flashcards.Where(e => e.UserId == userId).ToListAsync();
        }

        public async Task<List<FlashcardItem>?> GetFlashcardItemsById(int flashcardsId)
        {
            return await _context.FlashcardItems.Where(e => e.FlashcardId == flashcardsId).ToListAsync();
        }
    }
}
