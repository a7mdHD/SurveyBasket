using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Errors;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly int _refreshTokenExpiration = 14;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        var refreshToken = RefreshTokenGeneration();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiration);

        return new AuthResponse
        (
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            token,
            expiresIn,
            refreshToken,
            refreshTokenExpiry
        );
    }


    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

        // generate token

        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        var refreshToken = RefreshTokenGeneration();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiration);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenExpiry,
        });
        await _userManager.UpdateAsync(user);

        var response =  new AuthResponse
        (
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            token,
            expiresIn,
            refreshToken,
            refreshTokenExpiry
        );
        return Result.Success<AuthResponse>(response);
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure<AuthResponse>(UserError.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserError.InvalidJwtToken);

        var userRefreshToken = user.RefreshTokens
            .SingleOrDefault(t => t.Token == refreshToken && t.IsActive);

        if (userRefreshToken is null)
            return Result.Failure<AuthResponse>(UserError.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
        var newRefreshToken = RefreshTokenGeneration();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiration);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExpiry,
        });
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        (
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            newToken,
            expiresIn,
            newRefreshToken,
            refreshTokenExpiry
        );

        return Result.Success<AuthResponse>(response);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure(UserError.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserError.InvalidJwtToken);

        var userRefreshToken = user.RefreshTokens
            .SingleOrDefault(t => t.Token == refreshToken && t.IsActive);

        if (userRefreshToken is null)
            return Result.Failure(UserError.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    private static string RefreshTokenGeneration()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

}
