using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserContextService _userContextService;

    public UserController(IUserService userService, IUserContextService userContextService)
    {
        _userService = userService;
        _userContextService = userContextService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(UserLoginDto dto)
    {
        var token = await _userService.LoginAndGenerateJwtTokenAsync(dto);
        if (token.Success) return Ok(token.Value);
        return Unauthorized(token.ErrorMessage);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDto dto)
    {
        var registrationError = await _userService.RegisterUserAsync(dto);
        if (registrationError is null)
            return Ok();
        return BadRequest(registrationError);
    }

    [HttpPost("confirmAccount/{code}")]
    public async Task<ActionResult> ConfirmAccount(UserRegisterDto dto, [FromRoute] string code)
    {
        bool confirmed = dto.Email != null && await _userService.ConfirmAccountAsync(dto.Email, code);
        if (confirmed) return await Login(new UserLoginDto { Email = dto.Email, Password = dto.Password });
        return BadRequest(UserErrorMessages.AccountNotConfirmed);
    }

    [HttpPost("forgotPassword")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var result = await _userService.GenerateCodeForNewPasswordAsync(dto.Email!);
        return result == null ? Ok() : BadRequest(result);
    }

    [HttpPost("forgotPassword/{code}")]
    public async Task<ActionResult> ForgotPasswordCodeConfirmation(ForgotPasswordDto dto, string code)
    {
        var result = await _userService.CheckPasswordCodeValidityAsync(dto.Email!, code);

        return result == null ? Ok() : BadRequest(result);
    }

    [HttpPost("forgotPassword/changePassword")]
    public async Task<ActionResult> ChangePassword(ForgotPasswordDto dto)
    {
        var result = await _userService.ResetPasswordAsync(dto);

        return result == null ? Ok() : BadRequest(result);
    }

    [Authorize]
    [HttpGet("getUserProfile")]
    public async Task<ActionResult> GetUserProfileInformation()
    {
        var userId = _userContextService.UserId;
        if (userId is null)
            return Unauthorized();

        var userResult = await _userService.GetUserProfileInformationAsync(userId.Value);

        if (userResult is null)
            return BadRequest();

        return Ok(userResult);
    }

    [Authorize]
    [HttpPut("edit")]
    public async Task<ActionResult> ChangeUserProperties(ChangeUserPropertiesDto dto)
    {
        var userId = _userContextService.UserId;
        if (userId is null)
            return BadRequest();

        var result = await _userService.ChangeUserPropertiesAsync(userId.Value, dto);
        if (result is null) return Ok();
        return BadRequest(result);
    }

    [Authorize]
    [HttpPost("changePassword")]
    public async Task<ActionResult> ChangePassword(ChangeUserPasswordDto dto)
    {
        if (HttpContext.User.Identity is not ClaimsIdentity identity) return BadRequest(UserErrorMessages.PersonWithThisIdDoesNotExist);

        var claims = identity.Claims;

        var userId = claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return BadRequest();
        var changePasswordResult = await _userService.ChangePasswordAsync(int.Parse(userId), dto);

        if (changePasswordResult is null) return Ok();
        return BadRequest(changePasswordResult);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("getUserSessions/{username?}")]
    public async Task<ActionResult> GetUserSessions(string? username)
    {
        var result = await _userService.GetUserSessionsAsync(username);

        return result != null ? Ok(result) : BadRequest();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("getAllUsers")]
    public async Task<ActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return result != null ? Ok(result) : BadRequest();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{userId:int}")]
    public async Task<ActionResult> DeleteUser(int userId)
    {
        var loggedInUserId = _userContextService.UserId;
        if (loggedInUserId == null) return Unauthorized();
        if (loggedInUserId == userId) return BadRequest();

        await _userService.DeleteUserByIdAsync(userId);
        return Ok();
    }

    [Authorize]
    [HttpGet("jwtVerify")]
    public ActionResult JwtVerify()
    {
        return Ok();
    }
}