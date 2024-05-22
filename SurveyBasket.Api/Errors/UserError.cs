namespace SurveyBasket.Api.Errors;

public static class UserError
{
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "User or password is invalid!");
}
