using QuizPlatform.Infrastructure.Models.Question;
namespace QuizPlatform.Infrastructure.Models.Test;

public class TestDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    public bool OneQuestionMode { get; set; }
    public List<QuestionDto>? Questions { get; set; }
}