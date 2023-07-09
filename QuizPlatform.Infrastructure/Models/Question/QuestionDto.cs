namespace QuizPlatform.Infrastructure.Models.Question;

public class QuestionDto
{
    public string? Question { get; set; }
    public int QuestionType { get; set; }
    public ICollection<string>? Answers { get; set; }
}