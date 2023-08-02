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

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(UserLoginDto dto)
    {
        var token = await _userService.LoginAndGenerateJwtTokenAsync(dto);
        if (token is null) return Unauthorized("Bad username or password.");
        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDto dto)
    {
        var registrationError = await _userService.RegisterUserAsync(dto);
        if (registrationError is null)
            return await Login(new UserLoginDto { Email = dto.Email, Password = dto.Password });
        return BadRequest(registrationError);
    }

    [Authorize]
    [HttpPost("changePassword")]
    public async Task<ActionResult> ChangePassword(ChangeUserPasswordDto dto)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        if (identity == null) return BadRequest(UserErrorMessages.PersonWithThisIdDoesNotExist);

        var claims = identity.Claims;

        var userId = claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return BadRequest();
        var changePasswordResult = await _userService.ChangePasswordAsync(int.Parse(userId), dto);

        if (changePasswordResult is null) return Ok();
        return BadRequest(changePasswordResult);
    }
}