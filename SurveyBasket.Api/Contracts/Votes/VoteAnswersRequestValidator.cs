using FluentValidation;

namespace SurveyBasket.Api.Contracts.Votes;

public class VoteAnswersRequestValidator : AbstractValidator<VoteAnswersRequest>
{
    public VoteAnswersRequestValidator()
    {
        RuleFor(x => x.QuestionId).GreaterThan(0);
        RuleFor(x => x.AnswerId).GreaterThan(0);
    }
}
