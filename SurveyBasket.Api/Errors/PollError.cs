namespace SurveyBasket.Api.Errors;

public class PollError
{
    public static readonly Error PollNotFound = new("Poll.NotFound", "No poll was found with gien ID");
}
