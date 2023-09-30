using QuizPlatform.Infrastructure.Enums;

namespace QuizPlatform.Infrastructure.Models.Question;

public class CreateQuestionDto
{
    public string? Question { get; set; }
    public bool MathMode { get; set; }
    public bool IsDeleted { get; set; } = false;
    public QuestionType QuestionType { get; set; }
    public List<CreateAnswerDto>? Answers { get; set; }
}