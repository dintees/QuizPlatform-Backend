namespace QuizPlatform.Infrastructure.Entities
{
    public class FlashcardItem : Entity
    {
        public string? FirstSide { get; set; }
        public string? SecondSide { get; set; }
        public Flashcard? Flashcard { get; set; }
        public int FlashcardId { get; set; }
    }
}
