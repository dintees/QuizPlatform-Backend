using QuizPlatform.Infrastructure.Enums;

namespace QuizPlatform.Infrastructure.Entities;

public class Question
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public QuestionTypeName QuestionType { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<QuestionAnswer>? Answers { get; set; }
    public ICollection<QuestionSet>? Sets { get; set; }
}