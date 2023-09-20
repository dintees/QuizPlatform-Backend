namespace QuizPlatform.Infrastructure.Entities;

public class Test : Entity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Question>? Questions { get; set; }
    //public ICollection<QuestionSet>? Questions { get; set; }
}