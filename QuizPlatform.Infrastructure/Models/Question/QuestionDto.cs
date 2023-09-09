using QuizPlatform.Infrastructure.Enums;

namespace QuizPlatform.Infrastructure.Models.Question;

public class QuestionDto
{
    public int Id { get; set; }
    public string? Question { get; set; }
    public QuestionTypeName QuestionType { get; set; }
    public List<CreateAnswerDto>? Answers { get; set; }
}