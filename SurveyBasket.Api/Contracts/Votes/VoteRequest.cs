namespace SurveyBasket.Api.Contracts.Votes;

public record VoteRequest(
    IEnumerable<VoteAnswersRequest> Answers
);
