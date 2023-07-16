using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Models.Set;

public class SetDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<QuestionDto>? Questions { get; set; }
}