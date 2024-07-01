namespace SurveyBasket.Api.Contracts.Results;

public record VoteResponse(
   string VoterName,
   DateTime VoteDate,
   IEnumerable<QuestionAnswerRersponse> SelectedAnswers
);