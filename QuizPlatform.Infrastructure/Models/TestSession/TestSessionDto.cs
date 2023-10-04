using QuizPlatform.Infrastructure.Models.Test;

namespace QuizPlatform.Infrastructure.Models.TestSession
{
    public class TestSessionDto : TestDto
    {
        public bool OneQuestionMode { get; set; }
        public bool IsCompleted { get; set; }
    }
}
