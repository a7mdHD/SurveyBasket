namespace SurveyBasket.Api.Contracts.Questions;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
	public QuestionRequestValidator()
	{
		RuleFor(x => x.Content)
			.NotEmpty()
			.Length(3, 1000);

		RuleFor(x => x.Answers)
			.NotNull();

		RuleFor(x => x.Answers)
			.Must(a => a.Count > 1)
			.WithMessage("Question should has at least 2 answers!")
			.When(x => x.Answers != null);


        RuleFor(x => x.Answers)
            .Must(a => a.Distinct().Count() == a.Count)
            .WithMessage("You cannot add duplicated answers for the same question!")
            .When(x => x.Answers != null);
    }
}
