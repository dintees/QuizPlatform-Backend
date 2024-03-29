﻿namespace QuizPlatform.Infrastructure.Entities
{
    public class Flashcard : Entity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public User? User { get; set; }
        public int UserId { get; set; }
        public ICollection<FlashcardItem>? FlashcardItems { get; set; }
    }
}
