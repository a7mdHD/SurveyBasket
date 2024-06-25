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
            : authResult.ToProblem();

            //: Problem(
            //    statusCode: StatusCodes.Status400BadRequest,
            //    title: "Bad Request",
            //    detail: authResult.Error.Message
            //);
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authResult = await _authService.GetRefreshTokenAsync(request.Token,
            request.RefreshToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }

    [HttpPost("revokeRefreshToken")]
    public async Task<IActionResult> revokeRefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request.Token,
            request.RefreshToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}
