namespace QuizPlatform.Infrastructure.Models.Question;

public class CreateQuestionDto
{
    public string? Question { get; set; }
    public int QuestionType { get; set; }
    public CreateAnswerDto[]? Answers { get; set; }
}