using QuizPlatform.Infrastructure.Enums;

namespace QuizPlatform.Infrastructure.Entities;

public class Question
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public QuestionType QuestionType { get; set; }
    public bool IsDeleted { get; set; }
    public bool MathMode { get; set; }
    public Test? Test { get; set; }
    public int TestId { get; set; }
    public ICollection<QuestionAnswer>? Answers { get; set; }

    //public ICollection<Test>? Tests { get; set; }
    //public ICollection<QuestionSet>? Tests { get; set; }
}