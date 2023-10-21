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

        public Task<Flashcard?> GetFlashcardsSetByIdAsync(int id)
        {
            return _context.Flashcards.Include(e => e.FlashcardItems).FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Flashcard flashcard)
        {
            await _context.Flashcards.AddAsync(flashcard);
        }

        public void Update(Flashcard flashcard)
        {
            _context.Entry(flashcard).State = EntityState.Modified;
        }

        public void Delete(FlashcardItem item)
        {
            _context.Entry(item).State = EntityState.Deleted;
        }

        public void Delete(Flashcard flashcard)
        {
            _context.Entry(flashcard).State = EntityState.Deleted;
        }

        public async Task<bool> SaveAsync()
        {
            foreach (var entityEntry in _context.ChangeTracker.Entries<Entity>())
            {
                var now = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                    entityEntry.Property(e => e.TsInsert).CurrentValue = now;
                entityEntry.Property(e => e.TsUpdate).CurrentValue = now;
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
