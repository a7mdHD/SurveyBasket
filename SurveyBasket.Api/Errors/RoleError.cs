namespace SurveyBasket.Api.Errors;

public class RoleError
{
    public static readonly Error RoleNotFound =
        new("Role.NotFound", "No role was found with gien ID", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedRole =
        new("Role.DuplicatedRole", "There is role with the same name is found!", StatusCodes.Status409Conflict);

    public static readonly Error InvalidPermissions =
    new("Role.InvalidPermissions", "Invalid permissions!", StatusCodes.Status400BadRequest);
}
