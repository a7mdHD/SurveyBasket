using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Api.Contracts.Auth;

public class RegisterRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string UserName { get; set; } = string.Empty;
    [Required, StringLength(280)]
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(250)]
    public string Password { get; set; } = string.Empty;
}
