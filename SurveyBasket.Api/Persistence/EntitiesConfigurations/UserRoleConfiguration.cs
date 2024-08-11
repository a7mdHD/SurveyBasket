namespace SurveyBasket.Api.Persistence.EntitiesConfigurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        // Default Data

        builder.HasData(
            new IdentityUserRole<string>
            {
                RoleId = DefaultRoles.AdminRoleId,
                UserId = DefaultUsers.AdminId,
            }    
        );
    }
}
