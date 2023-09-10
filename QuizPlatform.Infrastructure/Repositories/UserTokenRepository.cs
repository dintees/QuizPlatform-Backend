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

        //public async Task<IEnumerable<UserToken>> GetAllAsync(Func<UserToken, bool>? predicate)
        //{
        //    if (predicate != null) return _context.UserTokens.Where(predicate);

        //    return await _context.UserTokens.ToListAsync();
        //}

        //public async Task<UserToken>? GetOneAsync(Func<UserToken, bool>? predicate)
        //{
        //    if (predicate != null) return _context.UserTokens.FirstOrDefault(predicate);
        //    return null;
        //}

        //public void AddAsync(UserToken entity)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SaveAsync()
        //{
        //    throw new NotImplementedException();
        //}
        //}

        // TODO refactor
        public async Task<UserToken?> GetByUserIdAsync(int id)
        {
            return await _context.UserTokens.FirstOrDefaultAsync(e => e.UserId == id);
        }

        public async Task AddAsync(UserToken entity)
        {
            await _context.UserTokens.AddAsync(entity);
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
