namespace SurveyBasket.Api.Errors;

public static class UserError
{
    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "User or password is invalid!", StatusCodes.Status401Unauthorized);

    public static readonly Error DisabledUser =
       new("User.DisabledUser", "Disabled user please contact your administrator!", StatusCodes.Status401Unauthorized);

    public static readonly Error LockedUser =
      new("User.LockedUser", "Locked user please contact your administrator!", StatusCodes.Status401Unauthorized);

    public static readonly Error UserNotFound =
        new("User.UserNotFound", "User not found!", StatusCodes.Status404NotFound);

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicatedEmail =
        new("User.DuplicatedEmail", "This email already exists!", StatusCodes.Status409Conflict);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidConfirmationCode =
       new("User.InvalidConfirmationCode", "Invalid Confirmation Code", StatusCodes.Status400BadRequest);

    public static readonly Error EmailAlreadyConfirmed =
       new("User.EmailAlreadyConfirmed", "Email is already confirmed", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRoles =
        new("User.InvalidRoles", "Invalid Roles!", StatusCodes.Status400BadRequest);
}
