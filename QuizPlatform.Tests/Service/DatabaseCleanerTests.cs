using System.Linq.Expressions;
using Moq;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Service;

namespace QuizPlatform.Tests.Service
{
    public class DatabaseCleanerTests
    {
        [Fact]
        public async Task CleanUserTokensEntityAsync_ShouldCleanExpiredUserTokens()
        {
            // Arrange
            var userTokenRepositoryMock = new Mock<IUserTokenRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var expiredUserTokens = new List<UserToken>
            {
                new UserToken { Id = 1, UserId = 1, ExpirationTime = DateTime.Now.AddDays(-1), UserTokenType = UserTokenType.Registration },
                new UserToken { Id = 2, UserId = 2, ExpirationTime = DateTime.Now.AddMinutes(-30), UserTokenType = UserTokenType.PasswordReminder },
            };

            userTokenRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<UserToken, bool>>>())).ReturnsAsync(expiredUserTokens);
            userTokenRepositoryMock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(new User());

            var databaseCleaner = new DatabaseCleaner(userTokenRepositoryMock.Object, userRepositoryMock.Object);

            // Act
            await databaseCleaner.CleanUserTokensEntityAsync();

            // Assert
            userTokenRepositoryMock.Verify(x => x.DeleteToken(It.IsAny<UserToken>()), Times.Exactly(2));
            userRepositoryMock.Verify(x => x.DeleteUser(It.IsAny<User>()), Times.Once);
            userTokenRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }
    }
}
