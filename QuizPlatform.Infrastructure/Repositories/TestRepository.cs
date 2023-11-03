using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class TestRepository : ITestRepository
    {
        private readonly ApplicationDbContext _context;

        public TestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Test?> GetTestWithQuestionsByIdAsync(int id, bool readOnly = true)
        {
            if (readOnly) _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var set = await _context.Tests.AsSplitQuery()
            .Include(e => e.Questions.Where(q => !q.IsDeleted))!.ThenInclude(e => e!.Answers)
            .FirstOrDefaultAsync(s => s.Id == id);

            return set;
        }

        public async Task<Test?> GetByIdAsync(int id, bool readOnly = true)
        {
            if (readOnly) _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return await _context.Tests.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Test>?> GetTestsByUserIdAsync(int? userId)
        {
            // for admin - list all users
            if (userId is null)
                return await _context.Tests.Include(e => e.User).Where(e => !e.IsDeleted).ToListAsync();

            // for single user -> via user id
            return await _context.Tests.Include(e => e.User).Where(e => e.UserId == userId && !e.IsDeleted).ToListAsync();
        }

        public async Task AddAsync(Test test)
        {
            await _context.Tests.AddAsync(test);
        }

        public void Update(Test test)
        {
            _context.Entry(test).State = EntityState.Modified;
        }

        public async Task<bool> SaveAsync()
        {
            foreach (var entityEntry in _context.ChangeTracker.Entries<Entity>())
            {
                var now = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                    entityEntry.Property(e => e.TsInsert).CurrentValue = now;

                if (entityEntry.Entity.GetType() != typeof(TestSession))
                    entityEntry.Property(e => e.TsUpdate).CurrentValue = now;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Test>?> GetPublicTestsListAsync()
        {
            return await _context.Tests.Include(e => e.User).Where(e => e.IsPublic && !e.IsDeleted).ToListAsync();
        }
    }
}
