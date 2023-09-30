using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class UserAnswersRepository : IUserAnswersRepository
    {
        private readonly ApplicationDbContext _context;

        public UserAnswersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserAnswers?> GetUserAnswerByIdAsync(int id)
        {
            return await _context.UserAnswers.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<UserAnswers>?> GetUserAnswersByTestSessionIdAsync(int testSessionId)
        {
            return await _context.UserAnswers.Where(e => e.TestSessionId == testSessionId).ToListAsync();
        }

        public async Task AddAsync(UserAnswers userAnswer)
        {
            await _context.UserAnswers.AddAsync(userAnswer);
        }

        public async Task AddRangeAsync(List<UserAnswers> userAnswers)
        {
            await _context.UserAnswers.AddRangeAsync(userAnswers);
        }

        public void Update(UserAnswers userAnswer)
        {
            _context.Entry(userAnswer).State = EntityState.Modified;
        }

        public void Delete(UserAnswers userAnswers)
        {
            _context.Entry(userAnswers).State = EntityState.Deleted;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
