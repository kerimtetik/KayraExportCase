using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Identity;
using Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthDbContext _authDbContext;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        AuthDbContext authDbContext,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _authDbContext = authDbContext;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var emailExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email);
        if (emailExists)
        {
            return BadRequest("Bu e-posta zaten kayıtlı.");
        }

        var userNameExists = await _userManager.Users.AnyAsync(x => x.UserName == request.UserName);
        if (userNameExists)
        {
            return BadRequest("Bu kullanıcı adı zaten kayıtlı.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        return Ok("Kullanıcı oluşturuldu.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized("E-posta veya şifre hatalı.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return Unauthorized("E-posta veya şifre hatalı.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles
        );

        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = _tokenService.GetRefreshTokenExpiryUtc(),
            IsRevoked = false
        };

        _authDbContext.RefreshTokens.Add(refreshToken);
        await _authDbContext.SaveChangesAsync();

        return Ok(new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = _tokenService.GetAccessTokenExpiryUtc()
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshTokenRequestDto request)
    {
        var existingRefreshToken = await _authDbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsRevoked);

        if (existingRefreshToken is null || existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized("Refresh token geçersiz veya süresi dolmuş.");
        }

        var user = await _userManager.FindByIdAsync(existingRefreshToken.UserId);
        if (user is null)
        {
            return Unauthorized("Kullanıcı bulunamadı.");
        }

        existingRefreshToken.IsRevoked = true;

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles
        );

        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = user.Id,
            ExpiresAt = _tokenService.GetRefreshTokenExpiryUtc(),
            IsRevoked = false
        };

        _authDbContext.RefreshTokens.Add(newRefreshToken);
        await _authDbContext.SaveChangesAsync();

        return Ok(new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = _tokenService.GetAccessTokenExpiryUtc()
        });
    }

    [Authorize]
    [HttpGet("secure-ping")]
    public IActionResult SecurePing()
    {
        return Ok("Token geçerli.");
    }
}