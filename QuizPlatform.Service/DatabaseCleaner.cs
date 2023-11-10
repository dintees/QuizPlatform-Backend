using QuizPlatform.Infrastructure.Enums;
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

        /// <summary>
        /// Cleans all expires tokens for which the validity period has expired from the database
        /// </summary>
        /// <returns></returns>
        public async Task CleanUserTokensEntityAsync()
        {
            var expiredUserTokens = await _userTokenRepository.GetAllAsync(e => e.ExpirationTime < DateTime.Now);
            if (expiredUserTokens == null) return;

            foreach (var expiredUserToken in expiredUserTokens)
            {
                _userTokenRepository.DeleteToken(expiredUserToken);

                if (expiredUserToken.UserTokenType != UserTokenType.Registration) continue;
                var user = await _userRepository.GetUserByIdAsync(expiredUserToken.UserId);
                if (user is not null)
                    _userRepository.DeleteUser(user);
            }

            await _userTokenRepository.SaveAsync();
            Console.WriteLine("Cleaned userTokens entity.");
        }
    }
}
