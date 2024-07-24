using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Users;
using SurveyBasket.Api.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SurveyBasket.Api.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IEmailSender emailService,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailSender _emailService = emailService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly int _refreshTokenExpiration = 14;

    public async Task<Result> RegisterAsync(RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailIsExists)
            return Result.Failure(UserError.DuplicatedEmail);

        //var user = new ApplicationUser()
        //{
        //    FirstName = request.FirstName,
        //    LastName = request.LastName,
        //    Email = request.Email,
        //    UserName = request.Email,
        //}; 

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);


        if(result.Succeeded)
        {
            var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationCode));

            _logger.LogInformation("Confirmation Code : {confirmationCode}", confirmationCode);           

            await SendConfirmationEmail(user, confirmationCode);

            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user is null)
            return Result.Failure(UserError.InvalidConfirmationCode);

        if(user.EmailConfirmed)
            return Result.Failure(UserError.EmailAlreadyConfirmed);

        var code = request.Code;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserError.InvalidConfirmationCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);


        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }


    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(UserError.EmailAlreadyConfirmed);

        var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationCode));

        _logger.LogInformation("Confirmation Code : {confirmationCode}", confirmationCode);

        await SendConfirmationEmail(user, confirmationCode);

        return Result.Success();
    }
    
    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

        //var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

        //if (!isValidPassword)
        //    return Result.Failure<AuthResponse>(UserError.InvalidCredentials);

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if(result.Succeeded)
        {
            var response = await GetAuthResponse(user);
            return Result.Success(response);
        }

        return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserError.EmailNotConfirmed : UserError.InvalidCredentials);
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

        var response = await GetAuthResponse(user);

        return Result.Success(response);
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


    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();

        var confirmationCode = await _userManager.GeneratePasswordResetTokenAsync(user);

        confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationCode));

        _logger.LogInformation("Confirmation Code : {confirmationCode}", confirmationCode);

        await SendResetPasswordToken(user, confirmationCode);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordCodeAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if(user is null || !user.EmailConfirmed)
            return Result.Failure(UserError.InvalidConfirmationCode);

        IdentityResult result;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch(FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }

    private async Task<AuthResponse> GetAuthResponse(ApplicationUser user)
    {
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


    private static string RefreshTokenGeneration()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string confirmationCode)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
               new Dictionary<string, string>
               {
                    {"{{name}}", user.FirstName },
                    {"{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={confirmationCode}"}
               });

        BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email!, "✅ Basket Survy: Email Confiramtion", emailBody));
        await Task.CompletedTask;
    }


    private async Task SendResetPasswordToken(ApplicationUser user, string confirmationCode)
    {
        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
               new Dictionary<string, string>
               {
                    {"{{name}}", user.FirstName },
                    {"[Product Name]", "Survy Basket" },
                    {"{{action_url}}", $"{origin}/auth/forgetpassword?email={user.Email}&code={confirmationCode}"}
               });

        BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email!, "✅ Survy Basket: Change Password", emailBody));
        await Task.CompletedTask;
    }
}
