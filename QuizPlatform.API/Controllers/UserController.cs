using System.Security.Claims;
using FluentValidation;
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

    [Authorize]
    [HttpPut("edit")]
    public async Task<ActionResult> ChangeUserProperties(ChangeUserPropertiesDto dto)
    {
        var userId = _userContextService.UserId;
        if (userId is null)
            return BadRequest();

        var result= await _userService.ChangeUserPropertiesAsync(userId.Value, dto);
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
}