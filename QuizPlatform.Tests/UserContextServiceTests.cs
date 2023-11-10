using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class UserContextServiceTests
    {
        [Fact]
        public void UserId_WhenUserIsLoggedIn_ShouldReturnUserId()
        {
            // Arrange
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1410"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContextAccessorMock.Setup(x => x.HttpContext!.User).Returns(userPrincipal);

            var userContextService = new UserContextService(httpContextAccessorMock.Object);

            // Act
            var userId = userContextService.UserId;

            // Assert
            Assert.Equal(1410, userId);
        }

        [Fact]
        public void UserId_WhenUserIsNotLoggedIn_ShouldReturnNull()
        {
            // Arrange
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext!.User).Returns((ClaimsPrincipal)null!);

            var userContextService = new UserContextService(httpContextAccessorMock.Object);

            // Act
            var userId = userContextService.UserId;

            // Assert
            Assert.Null(userId);
        }

        [Fact]
        public void RoleName_WhenUserIsLoggedIn_ShouldReturnRoleName()
        {
            // Arrange
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContextAccessorMock.Setup(x => x.HttpContext!.User).Returns(userPrincipal);

            var userContextService = new UserContextService(httpContextAccessorMock.Object);

            // Act
            var roleName = userContextService.RoleName;

            // Assert
            Assert.Equal("Admin", roleName);
        }

        [Fact]
        public void RoleName_WhenUserIsNotLoggedIn_ShouldReturnNull()
        {
            // Arrange
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext!.User).Returns((ClaimsPrincipal)null!);

            var userContextService = new UserContextService(httpContextAccessorMock.Object);

            // Act
            var roleName = userContextService.RoleName;

            // Assert
            Assert.Null(roleName);
        }
    }
}
