using AutoMapper;
using FluentValidation;
using Moq;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Authentication;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.User;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class UserTests
    {
        private IUserService _service;

        public UserTests()
        {
            var authenticationSettings = new AuthenticationSettings
            {
                ExpiresDays = 1,
                Issuer = "test.com",
                Key = "KEY___PRIVATE___KEY"
            };

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<MainProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();

            var loggingService = new Mock<ILoggingService>();

            var users = GetUsers();
            var userRepositoryMock = GetUserRepositoryMock(users);

            IValidator<UserRegisterDto> userRegisterValidator = new UserRegisterValidator();
            IValidator<ChangeUserPasswordDto> changeUserPasswordValidator = new ChangeUserPasswordValidator();

            _service = new UserService(authenticationSettings, mapper, loggingService.Object, userRepositoryMock.Object, userRegisterValidator, changeUserPasswordValidator);
        }

        [Theory]
        [InlineData("d@d.pl", "a")]
        [InlineData("a@a.pl", "b")]
        public async Task LoginAndGenerateJwtTokenAsync_ForInvalidUsernameOrPassword_ReturnsNull(string email, string password)
        {
            var user = new UserLoginDto { Email = email, Password = password };

            var login = await _service.LoginAndGenerateJwtTokenAsync(user);

            Assert.Null(login);
        }

        [Fact]
        public async Task LoginAndGenerateJwtTokenAsync_ForValidUsernameAndPassword_ReturnsToken()
        {
            var user = new UserLoginDto { Email = "a@a.pl", Password = "aaaaaaaa" };

            var login = await _service.LoginAndGenerateJwtTokenAsync(user);

            Assert.NotNull(login);
        }


        [Theory]
        [InlineData("test", null, "12345678", "12345678", (int)RoleTypeName.User, UserErrorMessages.EmptyEmail)]
        [InlineData(null, "test@test.pl", "12345678", "12345678", (int)RoleTypeName.User, UserErrorMessages.EmptyUsername)]
        [InlineData("test", "test@test.pl", null, null, (int)RoleTypeName.User, UserErrorMessages.EmptyPassword)]

        public async Task RegisterUserAsync_ForEmptyFields_ReturnsProperErrorMessage(string username, string email, string password, string passwordConfirmation, int roleId, string expectedResult)
        {
            var user = new UserRegisterDto { Username = username, Email = email, Password = password, PasswordConfirmation = passwordConfirmation, RoleId = roleId };

            var register = await _service.RegisterUserAsync(user);

            Assert.Equal(expectedResult, register);
        }


        [Theory]
        [InlineData("test", "test.pl", "12345678", "12345678", (int)RoleTypeName.User, UserErrorMessages.WrongEmailFormat)]
        [InlineData("test", "test@test.pl", "1234", "1234", (int)RoleTypeName.User, UserErrorMessages.TooShortPassword)]
        [InlineData("test", "test@test.pl", "12345678", "1234567890", (int)RoleTypeName.User, UserErrorMessages.NotTheSamePasswords)]
        [InlineData("AdamAbacki", "a@a.pl", "12345678", "12345678", (int)RoleTypeName.User, UserErrorMessages.UserAlreadyExistsError)]
        public async Task RegisterUserAsync_ForIncorrectValues_ReturnsProperErrorMessage(string username, string email, string password, string passwordConfirmation, int roleId, string expectedResult)
        {
            var user = new UserRegisterDto { Username = username, Email = email, Password = password, PasswordConfirmation = passwordConfirmation, RoleId = roleId };

            var register = await _service.RegisterUserAsync(user);

            Assert.Equal(expectedResult, register);
        }


        [Fact]
        public async Task RegisterUserAsync_ForProperUsernameEmailAndPasswords_ReturnsTrueAndRegisterUser()
        {
            var user = new UserRegisterDto { Username = "Test", Email = "test@test.pl", Password = "aaaaaaaa", PasswordConfirmation = "aaaaaaaa", RoleId = (int)RoleTypeName.User };

            var register = await _service.RegisterUserAsync(user);
            var registerConfirmation = await _service.RegisterUserAsync(user);
           
            Assert.Null(register);
            Assert.Equal(UserErrorMessages.UserAlreadyExistsError, registerConfirmation);
        }


        [Theory]
        [InlineData(20, "old", "new", "new", UserErrorMessages.PersonWithThisIdDoesNotExist)]
        [InlineData(1, "old", "new", "new", UserErrorMessages.TooShortPassword)]
        [InlineData(1, "old", "12345678", "123456789", UserErrorMessages.NotTheSamePasswords)]
        [InlineData(1, "old", "12345678", "12345678", UserErrorMessages.CurrentPasswordIsIncorrect)]
        public async Task ChangePassword_ForInvalidValues_ReturnsProperMessageError(int id, string oldPassword, string newPassword, string newPasswordConfirmation, string expectedResult)
        {
            var changeuserPassword = new ChangeUserPasswordDto { OldPassword = oldPassword, NewPassword = newPassword, NewPasswordConfirmation = newPasswordConfirmation };

            var changePassword = await _service.ChangePasswordAsync(id, changeuserPassword);

            Assert.Equal(expectedResult, changePassword);
        }

        [Fact]
        public async Task ChangePassword_ForProperValues_ReturnsNullAndChangePassword()
        {
            var changeuserPassword = new ChangeUserPasswordDto { OldPassword = "cccccccc", NewPassword = "dddddddd", NewPasswordConfirmation = "dddddddd" };

            var changePassword = await _service.ChangePasswordAsync(3, changeuserPassword);
            var incorrectLogin = await _service.LoginAndGenerateJwtTokenAsync(new UserLoginDto { Email = "c@c.pl", Password = "cccccccc"});
            var correctLogin = await _service.LoginAndGenerateJwtTokenAsync(new UserLoginDto { Email = "c@c.pl", Password = "dddddddd"});

            Assert.Null(changePassword);
            Assert.Null(incorrectLogin);
            Assert.NotNull(correctLogin);
        }


        private List<User> GetUsers()
        {
            var users = new List<User>
            {
                new User {Id = 1, Email = "a@a.pl", Username = "AdamAbacki", Password = HashPassword("aaaaaaaa"), Role = new Role { Name = RoleTypeName.Admin} },
                new User {Id = 2, Email = "b@b.pl", Username = "BartoszBabacki", Password = HashPassword("bbbbbbbb"), Role = new Role { Name = RoleTypeName.Teacher} },
                new User {Id = 3, Email = "c@c.pl", Username = "CezaryCadacki", Password = HashPassword("cccccccc"), Role = new Role { Name = RoleTypeName.User} },
            };
            return users;
        }

        private Mock<IUserRepository> GetUserRepositoryMock(List<User> users)
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(x => x.GetUserByEmail(It.IsAny<string>())).Returns((string email) => Task.FromResult(users.FirstOrDefault(e => e.Email == email)));
            userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<int>())).Returns((int id) => Task.FromResult(users.FirstOrDefault(e => e.Id == id)));
            userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns((string username, string email) => Task.FromResult(users.FirstOrDefault(e => e.Username == username || e.Email == email)));
            userRepositoryMock.Setup(x => x.AddNewUserAsync(It.IsAny<User>())).Returns((User user) => { users.Add(user); return Task.CompletedTask; });
            userRepositoryMock.Setup(x => x.SaveAsync()).Returns(Task.FromResult(true));
            return userRepositoryMock;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}