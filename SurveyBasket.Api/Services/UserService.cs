using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services;

public class UserService(UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    IRoleService roleService) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _context = context;
    private readonly IRoleService _roleService = roleService;

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await _userManager.Users.Where(x => x.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        //var user = await _userManager.FindByIdAsync(userId);

        //user = request.Adapt(user);

        //await _userManager.UpdateAsync(user!);

        // ************** To Improve Performance ************** //

        await _userManager.Users.Where(x => x.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.FirstName, request.FirstName)
                .SetProperty(x => x.LastName, request.LastName)
            );

        return Result.Success();
    }


    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default) => 
        await (from u in  _context.Users
                join ur in _context.UserRoles
                on u.Id equals ur.UserId
                join r in _context.Roles
                on ur.RoleId equals r.Id into roles
                where !roles.Any(role => role.Name == DefaultRoles.Member)
                select new { u.Id, u.FirstName, u.LastName, u.Email,u.IsDisabled,
                    Roles = roles.Select(role => role.Name!).ToList()
                })
                .GroupBy(user => new { user.Id, user.FirstName, user.LastName, user.Email,user.IsDisabled}
                )
                .Select(user => new UserResponse(
                    user.Key.Id,    
                    user.Key.FirstName,
                    user.Key.LastName,
                    user.Key.Email,
                    user.Key.IsDisabled,
                    user.SelectMany(user => user.Roles))
                )
                .ToListAsync(cancellationToken);



    public async Task<Result<UserResponse>> GetByIdAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserError.UserNotFound);

        var userRoles = await _userManager.GetRolesAsync(user);

        var response = (user,  userRoles).Adapt<UserResponse>();
        
        return Result.Success(response);
    }
       
    public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var isEmailExists = await _context.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if(isEmailExists)
            return Result.Failure<UserResponse>(UserError.DuplicatedEmail);

        var allowedRoles = await _roleService.GetAllAsync(false, cancellationToken);

        if(request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
            return Result.Failure<UserResponse>(UserError.InvalidRoles);
        

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if(result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, request.Roles);

            var resposne = (user, request.Roles).Adapt<UserResponse>();
            return Result.Success(resposne);
        }

        var error = result.Errors.First();

        return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }


    public async Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if(await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserError.UserNotFound);

        var isEmailExists = await _context.Users.AnyAsync(x => x.Email == request.Email && x.Id != id, cancellationToken);

        if (isEmailExists)
            return Result.Failure(UserError.DuplicatedEmail);

        var allowedRoles = await _roleService.GetAllAsync(false, cancellationToken);

        if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
            return Result.Failure(UserError.InvalidRoles);

        user = request.Adapt(user);
        
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _context.UserRoles.Where(x => x.UserId == id).ExecuteDeleteAsync(cancellationToken);

            await _userManager.AddToRolesAsync(user, request.Roles);

            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleStatus(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserError.UserNotFound);

        user.IsDisabled = !user.IsDisabled;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();
        
        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UnlockUser(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserError.UserNotFound);

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

}