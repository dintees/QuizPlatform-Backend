namespace QuizPlatform.Infrastructure.Models.Question;

public class CreateAnswerDto
{
    public string? Answer { get; set; }
    public bool Correct { get; set; }
}