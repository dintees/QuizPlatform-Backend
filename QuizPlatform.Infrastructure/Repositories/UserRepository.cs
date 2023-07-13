using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddNewUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Username == username);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task EditPassword(User user, string newPassword)
        {
            user.Password = newPassword;
            await _context.SaveChangesAsync();
        }
    }
}
