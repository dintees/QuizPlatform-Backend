namespace QuizPlatform.Infrastructure.Models.Question;

public class AnswerDto
{
    public int? Id { get; set; }
    public string? Answer { get; set; }
    public bool Correct { get; set; }
}