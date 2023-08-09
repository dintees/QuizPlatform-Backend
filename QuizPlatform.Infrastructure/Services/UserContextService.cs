using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using QuizPlatform.Infrastructure.Interfaces;

namespace EducationPlatform.Infrastructure.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public int? UserId =>
        int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;

    public string? RoleName => User?.FindFirst(ClaimTypes.Role)?.Value;
}