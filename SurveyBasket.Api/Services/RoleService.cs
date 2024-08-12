namespace SurveyBasket.Api.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext context) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<RoleResponse>>> GetAllAsync(bool? includeDisabled = false,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles
                    .Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisabled.HasValue && includeDisabled.Value)))
                    .ProjectToType<RoleResponse>()
                    .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<RoleResponse>>(roles);
    }

    public async Task<Result<RoleDetailsResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if(await  _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailsResponse>(RoleError.RoleNotFound);

        var permissions = await _roleManager.GetClaimsAsync(role);

        var resposne = new RoleDetailsResponse(role.Id, role.Name!, role.IsDeleted,permissions.Select(x => x.Value));

        return Result.Success(resposne);
    }

    public async Task<Result<RoleDetailsResponse>> AddAsync(RoleRequest request, CancellationToken cancellationToken = default)
    {
        var isRoleExist = await _roleManager.RoleExistsAsync(request.Name);

        if (isRoleExist)
            return Result.Failure<RoleDetailsResponse>(RoleError.DuplicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();

        if(request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailsResponse>(RoleError.InvalidPermissions);

        var role = new ApplicationRole
        {
            Name = request.Name,
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            NormalizedName = request.Name.ToUpper()
        };

        var result = await _roleManager.CreateAsync(role);

        if(result.Succeeded)
        {
            var permissions = request.Permissions
                .Select(permission => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = permission,
                    RoleId = role.Id
                });

            await _context.AddRangeAsync(permissions);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new RoleDetailsResponse(role.Id, role.Name, role.IsDeleted, request.Permissions);

            return Result.Success(response);
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailsResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }


    public async Task<Result> UpdateAsync(string id, RoleRequest request, CancellationToken cancellationToken = default)
    {

        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailsResponse>(RoleError.RoleNotFound);

        var isRoleExist = await _roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id);

        if (isRoleExist)
            return Result.Failure<RoleDetailsResponse>(RoleError.DuplicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();

        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailsResponse>(RoleError.InvalidPermissions);

        role.Name = request.Name;
        role.ConcurrencyStamp = request.Name.ToUpper();


        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {


            var currentPermissions = await _context.RoleClaims
                .Where(x => x.RoleId == id && x.ClaimType == Permissions.Type)
                .Select(x => x.ClaimValue)
                .ToListAsync(cancellationToken);

            var newPermissions = request.Permissions
                .Except(currentPermissions)
                .Select(permission => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = permission,
                    RoleId = role.Id
                });

            var removedPermissions = currentPermissions.Except(request.Permissions);

            await _context.RoleClaims
                .Where(x => x.RoleId == id && removedPermissions.Contains(x.ClaimValue))
                .ExecuteDeleteAsync(cancellationToken);

            await _context.AddRangeAsync(newPermissions);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new RoleDetailsResponse(role.Id, role.Name, role.IsDeleted, request.Permissions);

            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailsResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleStatusAsync(string id)
    {

        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailsResponse>(RoleError.RoleNotFound);

        role.IsDeleted = !role.IsDeleted;

        var result = await _roleManager.UpdateAsync(role);

         return Result.Success();
    }
}
