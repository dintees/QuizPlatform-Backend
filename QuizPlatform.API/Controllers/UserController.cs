using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UserRegisterDto> _userRegisterValidator;
    private readonly IValidator<ChangeUserPasswordDto> _changeUserPasswordValidator;

    public UserController(IUserService userService, IValidator<UserRegisterDto> userRegisterValidator, IValidator<ChangeUserPasswordDto> changeUserPasswordValidator)
    {
        _userService = userService;
        _userRegisterValidator = userRegisterValidator;
        _changeUserPasswordValidator = changeUserPasswordValidator;
    }
    

    [HttpGet("getAll")]
    public async Task<ActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        if (users is null) return NoContent();
        return Ok(users);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult> Login(UserLoginDto dto)
    {
        var token = await _userService.LoginAndGenerateJwtTokenAsync(dto);
        if (token is null) return Unauthorized();
        return Ok(token);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDto dto)
    {
        var validator = await _userRegisterValidator.ValidateAsync(dto);
        if (!validator.IsValid) { return BadRequest(validator); }

        bool isRegistered = await _userService.RegisterUserAsync(dto);
        if (isRegistered)
        {
            return await Login(new UserLoginDto { Email = dto.Email, Password = dto.Password });
        }
        return BadRequest();
    }
    
    [Authorize]
    [HttpPost("changePassword")]
    public async Task<ActionResult> ChangePassword(ChangeUserPasswordDto dto)
    {
        var validatorResults = await _changeUserPasswordValidator.ValidateAsync(dto);
        if (!validatorResults.IsValid) { return BadRequest(validatorResults); }

        var identity = HttpContext.User.Identity as ClaimsIdentity;
        if (identity == null) return BadRequest();
        var claims = identity.Claims;

        var userId = claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId != null && await _userService.ChangePassword(int.Parse(userId), dto)) return Ok();
        
        return BadRequest();
    }
}