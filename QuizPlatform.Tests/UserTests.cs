using AutoMapper;
using FluentValidation;
using Moq;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Authentication;
using QuizPlatform.Infrastructure.Builders;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Enums;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.User;
using QuizPlatform.Infrastructure.Profiles;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Tests
{
    public class UserTests
    {
        private readonly IUserService _userService;

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

            var userTokenRepositoryMock = new Mock<IUserTokenRepository>();
            userTokenRepositoryMock.Setup(x => x.GetByUserIdAndTypeAsync(It.IsAny<int>(), UserTokenType.Registration)).ReturnsAsync((int _, UserTokenType _) => new UserToken { ExpirationTime = DateTime.Now.AddMinutes(1), Token = "123456", UserId = 5, UserTokenType = UserTokenType.Registration });
            userTokenRepositoryMock.Setup(x => x.GetByUserIdAndTypeAsync(2, UserTokenType.PasswordReminder)).ReturnsAsync((int _, UserTokenType _) => new UserToken { ExpirationTime = DateTime.Now.AddMinutes(1), Token = "654321", UserId = 2, UserTokenType = UserTokenType.PasswordReminder });
            userTokenRepositoryMock.Setup(x => x.SaveAsync()).ReturnsAsync(true);

            var emailConfiguration = new EmailConfiguration
            {
                SmtpServer = null,
                Port = 123,
                From = "admin@fiszlet.com",
                FromDisplayName = "Administrator",
                UserName = "admin",
                Password = "password",
            };

            var emailBuilder = new Mock<EmailBuilder>(emailConfiguration);
            var emailService = new Mock<IEmailService>();

            _userService = new UserService(authenticationSettings, mapper, loggingService.Object, userRepositoryMock.Object, userTokenRepositoryMock.Object, emailBuilder.Object, emailService.Object, userRegisterValidator, changeUserPasswordValidator);
        }

        [Theory]
        [InlineData("d@d.pl", "a")]
        [InlineData("a@a.pl", "b")]
        public async Task LoginAndGenerateJwtTokenAsync_ForInvalidUsernameOrPassword_ReturnsResultWithProperErrorMessage(string email, string password)
        {
            // Arrange
            var user = new UserLoginDto { Email = email, Password = password };

            // Act
            var login = await _userService.LoginAndGenerateJwtTokenAsync(user);

            // Assert
            Assert.IsType<Result<string>>(login);
            Assert.False(login.Success);
            Assert.Equal(UserLoginErrorMessages.BadUserNameOrPassword, login.ErrorMessage);
        }

        [Fact]
        public async Task LoginAndGenerateJwtTokenAsync_ForNotConfirmedAccount_ReturnsResultWithProperErrorMessage()
        {
            // Arrange
            var user = new UserLoginDto { Email = "b@b.pl", Password = "bbbbbbbb" };

            // Act
            var login = await _userService.LoginAndGenerateJwtTokenAsync(user);

            // Assert
            Assert.IsType<Result<string>>(login);
            Assert.False(login.Success);
            Assert.Equal(UserLoginErrorMessages.AccountHasNotBeenConfirmed, login.ErrorMessage);
        }

        [Fact]
        public async Task LoginAndGenerateJwtTokenAsync_ForDeletedUser_ReturnsResultWithProperErrorMessage()
        {
            // Arrange
            var user = new UserLoginDto { Email = "d@d.pl", Password = "dddddddd" };

            // Act
            var login = await _userService.LoginAndGenerateJwtTokenAsync(user);

            // Assert
            Assert.IsType<Result<string>>(login);
            Assert.False(login.Success);
            Assert.Equal(UserLoginErrorMessages.BadUserNameOrPassword, login.ErrorMessage);
        }

        [Fact]
        public async Task LoginAndGenerateJwtTokenAsync_ForValidUsernameAndPassword_ReturnsToken()
        {
            // Arrange
            var user = new UserLoginDto { Email = "a@a.pl", Password = "aaaaaaaa" };

            // Act
            var login = await _userService.LoginAndGenerateJwtTokenAsync(user);

            // Assert
            Assert.IsType<Result<string>>(login);
            Assert.True(login.Success);
            Assert.NotEmpty(login.Value!);
            Assert.Null(login.ErrorMessage);
        }


        [Theory]
        [InlineData("", "Test", "Testowy", "test@test.pl", "12345678", "12345678", 1, UserErrorMessages.EmptyUserName)]
        [InlineData("test", "", "Testowy", "test@test.pl", "12345678", "12345678", 1, UserErrorMessages.EmptyFirstName)]
        [InlineData("test", "Test", "", "test@test.pl", "12345678", "12345678", 1, UserErrorMessages.EmptyLastName)]
        [InlineData("test", "Test", "Testowy", null, "12345678", "12345678", 1, UserErrorMessages.EmptyEmail)]
        [InlineData("test", "Test", "Testowy", "test@test.pl", null, null, 1, UserErrorMessages.EmptyPassword)]
        public async Task RegisterUserAsync_ForEmptyFields_ReturnsProperErrorMessage(string username, string firstname, string lastname, string email, string password, string passwordConfirmation, int roleId, string expectedResult)
        {
            // Arrange
            var user = new UserRegisterDto { UserName = username, FirstName = firstname, LastName = lastname, Email = email, Password = password, PasswordConfirmation = passwordConfirmation, RoleId = roleId };

            // Act
            var register = await _userService.RegisterUserAsync(user);

            // Assert
            Assert.Equal(expectedResult, register);
        }


        [Theory]
        [InlineData("test", "Test", "Test", "test.pl", "12345678", "12345678", 1, UserErrorMessages.WrongEmailFormat)]
        [InlineData("test", "Test", "Test", "test@test.pl", "1234", "1234", 1, UserErrorMessages.TooShortPassword)]
        [InlineData("test", "Test", "Test", "test@test.pl", "12345678", "1234567890", 1, UserErrorMessages.NotTheSamePasswords)]
        [InlineData("AdamAbacki", "Test", "Test", "a@a.pl", "12345678", "12345678", 1, UserErrorMessages.UserAlreadyExistsError)]
        public async Task RegisterUserAsync_ForIncorrectValues_ReturnsProperErrorMessage(string username, string firstname, string lastname, string email, string password, string passwordConfirmation, int roleId, string expectedResult)
        {
            // Arrange
            var user = new UserRegisterDto { UserName = username, FirstName = firstname, LastName = lastname, Email = email, Password = password, PasswordConfirmation = passwordConfirmation, RoleId = roleId };

            // Act
            var register = await _userService.RegisterUserAsync(user);

            // Assert
            Assert.Equal(expectedResult, register);
        }


        [Fact]
        public async Task RegisterUserAsync_ForTheSameUsernameAndPassword_ReturnsTrueAndRegisterUser()
        {
            // Arrange
            var user = new UserRegisterDto { UserName = "Test", FirstName = "Test", LastName = "Test", Email = "test@test.pl", Password = "aaaaaaaa", PasswordConfirmation = "aaaaaaaa", RoleId = 1 };

            // Act
            var register = await _userService.RegisterUserAsync(user);
            var registerConfirmation = await _userService.RegisterUserAsync(user);

            // Assert
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
            // Arrange
            var changeUserPassword = new ChangeUserPasswordDto { OldPassword = oldPassword, NewPassword = newPassword, NewPasswordConfirmation = newPasswordConfirmation };

            // Act
            var changePassword = await _userService.ChangePasswordAsync(id, changeUserPassword);

            // Assert
            Assert.Equal(expectedResult, changePassword);
        }

        [Fact]
        public async Task ConfirmAccountAsync_ForIncorrectCode_ReturnsFalse()
        {
            // Arrange
            var email = "e@e.pl";
            var code = "123455";
            var userLoginDto = new UserLoginDto { Email = email, Password = "eeeeeeee" };

            // Act
            var result = await _userService.ConfirmAccountAsync(email, code);
            var loginResult = await _userService.LoginAndGenerateJwtTokenAsync(userLoginDto);

            // Assert
            Assert.False(result);
            Assert.False(loginResult.Success);
            Assert.Equal(UserLoginErrorMessages.AccountHasNotBeenConfirmed, loginResult.ErrorMessage!);
        }

        [Fact]
        public async Task ConfirmAccountAsync_ForCorrectCode_ReturnsTrue()
        {
            // Arrange
            var email = "e@e.pl";
            var code = "123456";
            var userLoginDto = new UserLoginDto { Email = email, Password = "eeeeeeee" };

            // Act
            var result = await _userService.ConfirmAccountAsync(email, code);
            var loginResult = await _userService.LoginAndGenerateJwtTokenAsync(userLoginDto);

            // Assert
            Assert.True(result);
            Assert.True(loginResult.Success);
        }

        [Fact]
        public async Task ChangeUserPropertiesAsync_ForInvalidUserId_ReturnsProperErrorMessage()
        {
            // Arrange
            var accountProperties = new ChangeUserPropertiesDto { FirstName = "Adam", LastName = "Malysz" };

            // Act
            var result = await _userService.ChangeUserPropertiesAsync(100, accountProperties);

            // Assert
            Assert.Equal(UserErrorMessages.PersonWithThisIdDoesNotExist, result);
        }

        [Fact]
        public async Task ChangeUserPropertiesAsync_ForValidUserId_ReturnsNullAndChangeProperties()
        {
            // Arrange
            var accountProperties = new ChangeUserPropertiesDto { FirstName = "Adam", LastName = "Malysz" };

            // Act
            var result = await _userService.ChangeUserPropertiesAsync(4, accountProperties);
            var profileInformation = await _userService.GetUserProfileInformationAsync(4);

            // Assert
            Assert.Null(result);
            Assert.Equal("Adam", profileInformation?.Firstname);
            Assert.Equal("Malysz", profileInformation?.Lastname);
        }

        [Fact]
        public async Task GetUserProfileInformation_ForGivenUserId_ReturnsUserDtoWithUserProfileInformation()
        {
            // Arrange
            const int userId = 1;

            // Act
            var result = await _userService.GetUserProfileInformationAsync(userId);

            // Assert
            Assert.Equal("Adam", result?.Firstname);
            Assert.Equal("Abacki", result?.Lastname);
            Assert.Equal("AdamAbacki", result?.Username);
            Assert.Equal("a@a.pl", result?.Email);
        }

        [Fact]
        public async Task GenerateCodeForNewPasswordAsync_ForUserWithGivenEmail()
        {
            // Arrange
            const string email = "e@e.pl";

            // Act
            var result = await _userService.GenerateCodeForNewPasswordAsync(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CheckPasswordCodeValidityAsync_ForIncorrectCode_ReturnsProperErrorMessage()
        {
            // Arrange
            const string email = "b@b.pl";

            // Act
            var result = await _userService.CheckPasswordCodeValidityAsync(email, "012321");

            // Assert
            Assert.Equal(UserTokenErrorMessages.IncorrectCode, result);
        }

        [Fact]
        public async Task CheckPasswordCodeValidityAsync_ForCodeWhichNotExists_ReturnsNotFoundMessage()
        {
            // Arrange
            const string email = "c@c.pl";

            // Act
            var result = await _userService.CheckPasswordCodeValidityAsync(email, "654321");

            // Assert
            Assert.Equal(GeneralErrorMessages.NotFound, result);
        }

        [Fact]
        public async Task CheckPasswordCodeValidityAsync_ForCorrectCode_ReturnsNull()
        {
            // Arrange
            const string email = "b@b.pl";

            // Act
            var result = await _userService.CheckPasswordCodeValidityAsync(email, "654321");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ForInvalidData_ReturnsProperErrorMessage()
        {
            // Arrange
            var data = new ForgotPasswordDto { Email = "e@e.pl", NewPassword = "ee", NewPasswordConfirmation = "ee" };

            // Act
            var result = await _userService.ResetPasswordAsync(data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(UserErrorMessages.TooShortPassword, result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ForValidData_ChangePasswordAndReturnsNull()
        {
            // Arrange
            var data = new ForgotPasswordDto { Email = "f@f.pl", NewPassword = "qqqqqqqq", NewPasswordConfirmation = "qqqqqqqq" };
            var badCredentials = new UserLoginDto { Email = "f@f.pl", Password = "ffffffff" };
            var correctCredentials = new UserLoginDto { Email = "f@f.pl", Password = "qqqqqqqq" };

            // Act
            var result = await _userService.ResetPasswordAsync(data);
            var badLogin = await _userService.LoginAndGenerateJwtTokenAsync(badCredentials);
            var correctLogin = await _userService.LoginAndGenerateJwtTokenAsync(correctCredentials);

            // Assert
            Assert.Null(result);
            Assert.False(badLogin.Success);
            Assert.True(correctLogin.Success);
        }




        [Fact]
        public async Task ChangePassword_ForProperValues_ReturnsNullAndChangePassword()
        {
            // Arrange
            var changeUserPassword = new ChangeUserPasswordDto { OldPassword = "cccccccc", NewPassword = "dddddddd", NewPasswordConfirmation = "dddddddd" };

            // Act
            var changePassword = await _userService.ChangePasswordAsync(3, changeUserPassword);
            var incorrectLogin = await _userService.LoginAndGenerateJwtTokenAsync(new UserLoginDto { Email = "c@c.pl", Password = "cccccccc" });
            var correctLogin = await _userService.LoginAndGenerateJwtTokenAsync(new UserLoginDto { Email = "c@c.pl", Password = "dddddddd" });

            // Assert
            Assert.Null(changePassword);
            Assert.False(incorrectLogin.Success);
            Assert.True(correctLogin.Success);
        }


        private List<User> GetUsers()
        {
            var users = new List<User>
            {
                new User {Id = 1, Email = "a@a.pl", UserName = "AdamAbacki", FirstName = "Adam", LastName = "Abacki", Password = HashPassword("aaaaaaaa"), AccountConfirmed = true, Role = new Role { Id = 1, Name = "Admin"}, IsDeleted = false },
                new User {Id = 2, Email = "b@b.pl", UserName = "BartoszBabacki", FirstName = "Bartosz", LastName = "Babacki", Password = HashPassword("bbbbbbbb"), AccountConfirmed = false, Role = new Role { Id = 2, Name = "User"}, IsDeleted = false },
                new User {Id = 3, Email = "c@c.pl", UserName = "CezaryCadacki", FirstName = "Cezary", LastName = "Cadacki", Password = HashPassword("cccccccc"), AccountConfirmed = true, Role = new Role { Id = 2, Name = "User"}, IsDeleted = false },
                new User {Id = 4, Email = "d@d.pl", UserName = "DariuszDadacki", FirstName = "Dariusz", LastName = "Dadacki", Password = HashPassword("dddddddd"), AccountConfirmed = true, Role = new Role { Id = 2, Name = "User"}, IsDeleted = true },
                new User {Id = 5, Email = "e@e.pl", UserName = "EdwardEdacki", FirstName = "Edward", LastName = "Edacki", Password = HashPassword("eeeeeeee"), AccountConfirmed = false, Role = new Role { Id = 2, Name = "User"}, IsDeleted = false },
                new User {Id = 5, Email = "f@f.pl", UserName = "FabianFadacki", FirstName = "Fabian", LastName = "Fadacki", Password = HashPassword("ffffffff"), AccountConfirmed = true, Role = new Role { Id = 2, Name = "User"}, IsDeleted = false },
            };
            return users;
        }

        private Mock<IUserRepository> GetUserRepositoryMock(List<User> users)
        {
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns((string email, bool _) => Task.FromResult(users.FirstOrDefault(e => e.Email == email)));
            userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<bool>())).Returns((int id, bool _) => Task.FromResult(users.FirstOrDefault(e => e.Id == id)));
            userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<string>(), It.IsAny<string>())).Returns((string username, string email) => Task.FromResult(users.FirstOrDefault(e => e.UserName == username || e.Email == email)));
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