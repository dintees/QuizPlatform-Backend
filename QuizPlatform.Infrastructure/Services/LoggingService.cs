using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Services;

public class LoggingService : ILoggingService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public LoggingService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
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

    public async Task<List<UserSessionDto>?> GetUserSessionsList(string? username)
    {
        if (username is null)
            return _mapper.Map<List<UserSessionDto>>(await _context.UserSessions.Include(e => e.User).OrderByDescending(e => e.LoggedInTime).ToListAsync());
        return _mapper.Map<List<UserSessionDto>>(await _context.UserSessions.Include(e => e.User).Where(e => e.User!.UserName == username).OrderByDescending(e => e.LoggedInTime).ToListAsync());
    }
}