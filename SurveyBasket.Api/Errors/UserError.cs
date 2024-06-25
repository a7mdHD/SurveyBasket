namespace SurveyBasket.Api.Errors;

public static class UserError
{
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "User or password is invalid!", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Status401Unauthorized);


}
