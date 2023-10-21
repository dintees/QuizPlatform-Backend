namespace QuizPlatform.Infrastructure.Entities;

public class Test : Entity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPublic { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleAnswers { get; set; }
    public bool OneQuestionMode { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    //public ICollection<QuestionSet>? Questions { get; set; }
}