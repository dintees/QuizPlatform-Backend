using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Service
{
    public class DatabaseCleaner : IDatabaseCleaner
    {
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IUserRepository _userRepository;

        public DatabaseCleaner(IUserTokenRepository userTokenRepository, IUserRepository userRepository)
        {
            _userTokenRepository = userTokenRepository;
            _userRepository = userRepository;
        }

        public async Task CleanUserTokensEntity()
        {
            var expiredUserTokens = await _userTokenRepository.GetAllAsync(e => e.ExpirationTime < DateTime.Now);
            if (expiredUserTokens == null) return;

            foreach (var expiredUserToken in expiredUserTokens)
            {
                var user = await _userRepository.GetUserByIdAsync(expiredUserToken.UserId);
                _userTokenRepository.DeleteToken(expiredUserToken);

                if (user is not null)
                    _userRepository.DeleteUser(user);
            }

            await _userTokenRepository.SaveAsync();
            Console.WriteLine("Cleaned userTokens entity.");
        }

    }
}
