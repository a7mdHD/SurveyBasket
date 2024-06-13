namespace SurveyBasket.Api.Errors;

public class QuestionError
{
    public static readonly Error QuestionNotFound =
    new("Question.NotFound", "No Question was found with gien ID");


    public static readonly Error DuplicatedQuestionContent =
    new("Question.DuplicatedQuestionContent", "There is question with the same content!");
}
