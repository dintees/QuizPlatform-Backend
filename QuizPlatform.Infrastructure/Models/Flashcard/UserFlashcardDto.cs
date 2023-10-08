namespace QuizPlatform.Infrastructure.Models.Flashcard
{
    public class UserFlashcardDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime? TsInsert { get; set; }
        public DateTime? TsUpdate { get; set; }
    }
}
