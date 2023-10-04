using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class TestSessionRepository : ITestSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public TestSessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestSession?> GetBySessionIdAsync(int testSessionId, bool includeTest = false)
        {
            if (includeTest)
                return await _context.TestSessions.Include(e => e.Test).ThenInclude(t => t!.Questions).ThenInclude(q => q.Answers).FirstOrDefaultAsync(e => e.Id == testSessionId);
            return await _context.TestSessions.FirstOrDefaultAsync(e => e.Id == testSessionId);
        }

        public async Task<List<TestSession>> GetByUserIdWithTestAsync(int userId, bool sort = false)
        {
            if (sort)
                return await _context.TestSessions.Include(e => e.Test).Where(e => e.UserId == userId).OrderBy(e => e.IsCompleted).ThenByDescending(e => e.TsUpdate).ToListAsync();
            return await _context.TestSessions.Include(e => e.Test).Where(e => e.UserId == userId).ToListAsync();
        }

        public async Task AddAsync(TestSession testSession)
        {
            await _context.TestSessions.AddAsync(testSession);
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
