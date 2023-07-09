﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizPlatform.Infrastructure.Authentication;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly AuthenticationSettings _authenticationSettings;
    private readonly IMapper _mapper;
    private readonly ILoggingService _loggingService;

    public UserService(ApplicationDbContext context, AuthenticationSettings authenticationSettings, IMapper mapper, ILoggingService loggingService)
    {
        _context = context;
        _mapper = mapper;
        _authenticationSettings = authenticationSettings;
        _loggingService = loggingService;
    }

    public async Task<List<User>?> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users;
    }

    public async Task<string?> LoginAndGenerateJwtTokenAsync(UserLoginDto dto)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == dto.Email); //.Include(u => u.Role)
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) return null;


        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username!),
            //new Claim(ClaimTypes.Role, $"{user.Role.Name}"),
            new Claim(ClaimTypes.Email, $"{user.Email}")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.Key!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(_authenticationSettings.ExpiresDays);
        var token = new JwtSecurityToken(_authenticationSettings.Issuer, _authenticationSettings.Issuer, claims, expires: expires, signingCredentials: credentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // log information to UserSessions table
        await _loggingService.LogLoginInformation(user.Id);
        
        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> RegisterUserAsync(UserRegisterDto dto)
    {
        var user = _mapper.Map<User>(dto);
        if (user is null) return false;

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ChangePassword(int id, ChangeUserPasswordDto user)
    {
        var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (foundUser is null) return false;

        if (!BCrypt.Net.BCrypt.Verify(user.OldPassword, foundUser.Password)) return false;

        foundUser.Password = BCrypt.Net.BCrypt.HashPassword(user.NewPassword);

        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> CheckIfEmailExists(string email)
    {
        User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        return (user != null);
    }

    public async Task<bool> CheckIfUsernameExists(string username)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        return (user != null);
    }
    
}