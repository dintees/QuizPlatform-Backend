using System.Security.Claims;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IUserContextService
{
    ClaimsPrincipal? User { get; }
    int? UserId { get; }
    string? RoleName { get; }
}