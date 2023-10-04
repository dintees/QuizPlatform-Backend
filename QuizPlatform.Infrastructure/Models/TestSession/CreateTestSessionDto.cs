namespace QuizPlatform.Infrastructure.Models.TestSession
{
    public class CreateTestSessionDto
    {
        public int TestId { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleAnswers { get; set; }
        public bool OneQuestionMode { get; set; }
    }
}
