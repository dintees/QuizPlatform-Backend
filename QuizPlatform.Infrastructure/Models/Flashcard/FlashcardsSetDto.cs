namespace QuizPlatform.Infrastructure.Models.Flashcard
{
    public class FlashcardsSetDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<FlashcardItemDto>? FlashcardItems { get; set; }
    }
}
