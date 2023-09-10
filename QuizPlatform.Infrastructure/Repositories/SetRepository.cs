using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class SetRepository : ISetRepository
    {
        private readonly ApplicationDbContext _context;

        public SetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Set?> GetSetWithQuestionsByIdAsync(int id, bool readOnly = true)
        {
            if (readOnly) _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var set = await _context.Sets.AsSplitQuery()
            .Include(e => e.Questions)!.ThenInclude(e => e!.Answers)
            .FirstOrDefaultAsync(s => s.Id == id);

            return set;
        }

        public async Task<Set?> GetSetByIdAsync(int id, bool readOnly = true)
        {
            if (readOnly) _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return await _context.Sets.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Set>?> GetSetsByUserIdAsync(int userId)
        {
            return await _context.Sets.Include(e => e.User).Where(e => e.UserId == userId && !e.IsDeleted).ToListAsync();
        }

        public async Task InsertSetAsync(Set set)
        {
            await _context.Sets.AddAsync(set);
        }

        public void UpdateSet(Set set)
        {
            _context.Entry(set).State = EntityState.Modified;
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
