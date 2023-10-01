﻿namespace QuizPlatform.Infrastructure.Entities
{
    public class TestSession : Entity
    {
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleAnswers { get; set; }
        public bool OneQuestionMode { get; set; }
        public Test? Test{ get; set; }
        public int TestId { get; set; }
        public User? User { get; set; }
        public int UserId { get; set; }
        public bool IsFinished { get; set; }
    }
}