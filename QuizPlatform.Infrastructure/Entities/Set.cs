namespace QuizPlatform.Infrastructure.Entities;

public class Set
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public ICollection<QuestionSet>? Questions { get; set; }
}