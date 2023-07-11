using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Models.Question;

public class CreateQuestionDto
{
    public string? Question { get; set; }
    public QuestionTypeName QuestionType { get; set; }
    public List<CreateAnswerDto>? Answers { get; set; }
}