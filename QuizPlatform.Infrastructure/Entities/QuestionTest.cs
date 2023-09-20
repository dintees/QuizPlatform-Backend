namespace QuizPlatform.Infrastructure.Entities;

public class QuestionTest
{
    public int QuestionId { get; set; }
    public virtual Question? Question { get; set; }

    public int SetId { get; set; }
    public virtual Test? Set { get; set; }
}