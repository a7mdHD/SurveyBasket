namespace SurveyBasket.Api.Entities;

public sealed class Answer : AuditableEntity
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = default!;
}
