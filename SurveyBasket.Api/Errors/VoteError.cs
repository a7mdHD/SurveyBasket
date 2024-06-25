namespace SurveyBasket.Api.Errors;

public static class VoteError
{
    public static readonly Error DuplicatedVote =
        new("Vote.DuplicatedPollVote", "There is Vote with the same User!", StatusCodes.Status409Conflict);

    public static readonly Error InvalidQuestions =
    new("Vote.InvalidQuestions", "Invalid questions", StatusCodes.Status409Conflict);
}
