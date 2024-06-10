namespace SurveyBasket.Api.Errors;

public static class UserError
{
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "User or password is invalid!");

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token");

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid refresh token");


}
