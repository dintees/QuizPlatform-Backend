using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public UserTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserToken?> GetByUserIdAsync(int id)
        {
            return await _context.UserTokens.FirstOrDefaultAsync(e => e.UserId == id);
        }

        public async Task AddAsync(UserToken entity)
        {
            await _context.UserTokens.AddAsync(entity);
        }

        public void DeleteToken(UserToken userToken)
        {
            _context.Entry(userToken).State = EntityState.Deleted;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
