namespace SurveyBasket.Api.Contracts.Auth;

public record RefreshTokenRequest
(
    string Token,
    string RefreshToken
);
