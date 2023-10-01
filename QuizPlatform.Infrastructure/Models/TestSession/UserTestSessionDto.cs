﻿namespace QuizPlatform.Infrastructure.Models.TestSession
{
    public class UserTestSessionDto
    {
        public int Id { get; set; }
        public string? TestName { get; set; }
        public bool IsFinished { get; set; }
        public DateTime? TsInsert { get; set; }
        public DateTime? TsUpdate { get; set; }
    }
}