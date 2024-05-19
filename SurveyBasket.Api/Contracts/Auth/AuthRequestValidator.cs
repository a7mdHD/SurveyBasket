public class AuthRequestValidator : AbstractValidator<LoginRequest>
{
    public AuthRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password).NotEmpty();   
    }

}