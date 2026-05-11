using AutoMapper;
using InventorySalesManagementSystem.API.Middleware;
using InventorySalesManagementSystem.Application.DTOs.Auth;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Entities;
using InventorySalesManagementSystem.Infrastructure.JWT;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InventorySalesManagementSystem.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await unitOfWork.Users.Query().AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new AppException("A user with this email already exists.", System.Net.HttpStatusCode.Conflict);
        }

        var customerRole = await unitOfWork.Roles.Query()
            .FirstOrDefaultAsync(x => x.Name == "Customer", cancellationToken)
            ?? throw new AppException("Customer role is not configured.", System.Net.HttpStatusCode.InternalServerError);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Password = passwordHasher.HashPassword(request.Password),
            RoleId = customerRole.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = customerRole
        };

        await unitOfWork.Users.AddAsync(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User registered successfully with email {Email}.", email);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await unitOfWork.Users.Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
            ?? throw new AppException("Invalid email or password.", System.Net.HttpStatusCode.Unauthorized);

        if (!user.IsActive)
        {
            throw new AppException("This user account is inactive.", System.Net.HttpStatusCode.Forbidden);
        }

        if (!passwordHasher.VerifyPassword(request.Password, user.Password))
        {
            throw new AppException("Invalid email or password.", System.Net.HttpStatusCode.Unauthorized);
        }
        

        logger.LogInformation("User {Email} logged in successfully.", email);

        return BuildAuthResponse(user);
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        return new AuthResponseDto
        {
            Token = jwtTokenGenerator.GenerateToken(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.Name ?? string.Empty
        };
    }
}
