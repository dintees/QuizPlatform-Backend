namespace QuizPlatform.Infrastructure.Entities;

public class Question
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public int QuestionTypeId { get; set; }
    public virtual QuestionType? QuestionType { get; set; }
    public ICollection<QuestionAnswer>? Answers { get; set; }
    
    public ICollection<QuestionSet>? Sets { get; set; }
}