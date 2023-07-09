using Microsoft.AspNetCore.Http;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Services;

public class LoggingService : ILoggingService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task LogLoginInformation(int userId)
    {
        var context = _httpContextAccessor.HttpContext;
        var ipAddress = context?.Connection.RemoteIpAddress?.ToString();
        var userAgent = context?.Request.Headers["User-Agent"].ToString();

        var userSession = new UserSession()
        {
            IPAddress = ipAddress,
            Browser = userAgent,
            LoggedInTime = DateTime.Now,
            UserId = userId
        };
        
        await _context.UserSessions.AddAsync(userSession);
        await _context.SaveChangesAsync();
    }
    
}