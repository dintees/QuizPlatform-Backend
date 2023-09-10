using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using QuizPlatform.Infrastructure.Authentication;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AuthenticationSettings _authenticationSettings;
    private readonly IMapper _mapper;
    private readonly ILoggingService _loggingService;
    private readonly IUserRepository _userRepository;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IValidator<UserRegisterDto> _userRegisterValidator;
    private readonly IValidator<ChangeUserPasswordDto> _changeUserPasswordValidator;

    public UserService(AuthenticationSettings authenticationSettings, IMapper mapper, ILoggingService loggingService, IUserRepository userRepository, IUserTokenRepository userTokenRepository, IValidator<UserRegisterDto> userRegisterValidator, IValidator<ChangeUserPasswordDto> changeUserPasswordValidator)
    {
        _mapper = mapper;
        _authenticationSettings = authenticationSettings;
        _loggingService = loggingService;
        _userRepository = userRepository;
        _userTokenRepository = userTokenRepository;
        _userRegisterValidator = userRegisterValidator;
        _changeUserPasswordValidator = changeUserPasswordValidator;
    }

    public async Task<Result<string>> LoginAndGenerateJwtTokenAsync(UserLoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email!);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) return new Result<string> { Success = false, ErrorMessage = UserLoginErrorMessages.BadUserNameOrPassword };

        if (user.AccountConfirmed == false)
            return new Result<string>
            { Success = false, ErrorMessage = UserLoginErrorMessages.AccountHasNotBeenConfirmed };


        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("username", user.UserName!),
            new Claim(ClaimTypes.Role, user.Role?.Name!),
            new Claim("roleName", user.Role?.Name!),
            new Claim("email", user.Email!),
            new Claim("firstname",user.FirstName! ),
            new Claim("lastname",user.LastName! )
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.Key!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(_authenticationSettings.ExpiresDays);
        var token = new JwtSecurityToken(_authenticationSettings.Issuer, _authenticationSettings.Issuer, claims, expires: expires, signingCredentials: credentials);
        var tokenHandler = new JwtSecurityTokenHandler();

        // log information to UserSessions table
        await _loggingService.LogLoginInformation(user.Id);
        var generatedToken = tokenHandler.WriteToken(token);

        return new Result<string> { Success = true, Value = generatedToken };
    }

    public async Task<string?> RegisterUserAsync(UserRegisterDto dto)
    {
        var validationResult = await _userRegisterValidator.ValidateAsync(dto);
        if (!validationResult.IsValid) return validationResult.Errors.FirstOrDefault()?.ErrorMessage;

        if (await _userRepository.GetUserAsync(dto.UserName!, dto.Email!) != null) return UserErrorMessages.UserAlreadyExistsError;

        var user = _mapper.Map<User>(dto);
        if (user is null) return GeneralErrorMessages.GeneralError;

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        user.AccountConfirmed = false;
        await _userRepository.AddNewUserAsync(user);

        await _userRepository.SaveAsync(); //  ? null : GeneralErrorMessages.GeneralError;

        var a = new UserToken { Token = "12341234", UserId = user.Id, ExpirationTIme = DateTime.Now.AddMinutes(30) };
        await _userTokenRepository.AddAsync(a);
        return await _userRepository.SaveAsync() ? null : GeneralErrorMessages.GeneralError;
    }

    public async Task<bool> ConfirmAccountAsync(string email, string code)
    {
        var user = await _userRepository.GetUserByEmailAsync(email, false);
        if (user is null) return false;

        var correctConfirmationCode = await _userTokenRepository.GetByUserIdAsync(user.Id);
        if (correctConfirmationCode is null) return false;

        if (code != correctConfirmationCode.Token) return false;

        user.AccountConfirmed = true;

        // TODO delete token from UserTokens Entity
        return await _userRepository.SaveAsync();
    }

    public async Task<string?> ChangePasswordAsync(int id, ChangeUserPasswordDto user)
    {
        var foundUser = await _userRepository.GetUserByIdAsync(id);
        if (foundUser is null) return UserErrorMessages.PersonWithThisIdDoesNotExist;

        var validationResult = await _changeUserPasswordValidator.ValidateAsync(user);
        if (!validationResult.IsValid) return validationResult.Errors.FirstOrDefault()?.ErrorMessage;

        if (!BCrypt.Net.BCrypt.Verify(user.OldPassword, foundUser.Password)) return UserErrorMessages.CurrentPasswordIsIncorrect;

        foundUser.Password = BCrypt.Net.BCrypt.HashPassword(user.NewPassword);

        _userRepository.UpdateUser(foundUser);
        return await _userRepository.SaveAsync() ? null : GeneralErrorMessages.GeneralError;
    }
}