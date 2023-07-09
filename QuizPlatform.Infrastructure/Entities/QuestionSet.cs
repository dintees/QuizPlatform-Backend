namespace QuizPlatform.Infrastructure.Entities;

public class QuestionSet
{
    public int QuestionId { get; set; }
    public virtual Question? Question { get; set; }

    public int SetId { get; set; }
    public virtual Set? Set { get; set; }
}