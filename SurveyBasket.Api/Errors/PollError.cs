namespace SurveyBasket.Api.Errors;

public class PollError
{
    public static readonly Error PollNotFound =
        new("Poll.NotFound", "No poll was found with gien ID", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedPollTitle =
        new("Poll.DuplicatedPollTitle", "There is poll with the same title!", StatusCodes.Status409Conflict);
}
