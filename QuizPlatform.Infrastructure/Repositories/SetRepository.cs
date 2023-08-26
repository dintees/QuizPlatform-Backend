using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Set;

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

            var tmp = await _context.Sets.AsSplitQuery()
            .Include(e => e.Questions)!.ThenInclude(e => e!.Answers)//.ThenInclude(r => r.Question).ThenInclude(e => e!.Answers)
            .FirstOrDefaultAsync(s => s.Id == id);

            return tmp;
        }

        public async Task<Set?> GetSetByIdAsync(int id, bool readOnly = true)
        {
            if (readOnly) _context.ChangeTracker.QueryTrackingBehavior= QueryTrackingBehavior.NoTracking;
            return await _context.Sets.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Set>?> GetSetsByUserIdAsync(int userId)
        {
            return await _context.Sets.Where(e => e.UserId == userId).ToListAsync();
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
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
