using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Api.Contracts.Auth;

public record RegisterRequest
(
     string FirstName,
     string LastName,
     string Email,
     string Password 
);
