using SurveyBasket.Api.Services;

namespace SurveyBasket.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResult = await _authService.GetTokenAsync(request.Email,
            request.Password);

        return authResult.IsSuccess 
            ? Ok(authResult.Value) 
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: authResult.Error.Code, detail: authResult.Error.Message);
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authResult = await _authService.GetRefreshTokenAsync(request.Token,
            request.RefreshToken);

        return authResult is null ? BadRequest("invalid token!") : Ok(authResult);
    }

    [HttpPost("revokeRefreshToken")]
    public async Task<IActionResult> revokeRefreshToken([FromBody] RefreshTokenRequest request)
    {
        var isRevoked = await _authService.RevokeRefreshTokenAsync(request.Token,
            request.RefreshToken);

        return isRevoked ? Ok("revoked") : BadRequest("invalid token!");
    }
}
