namespace QuizPlatform.Infrastructure.Models.Test
{
    public class CreateTestSessionDto
    {
        public int TestId { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleAnswers { get; set; }
    }
}
