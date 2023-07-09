namespace QuizPlatform.Infrastructure.Entities;

public class QuestionAnswer
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public bool Correct { get; set; }
    // public int IdQuestion { get; set; }
    // public virtual Question? Question { get; set; }
}