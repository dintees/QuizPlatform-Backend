namespace QuizPlatform.Infrastructure.Models.Test
{
    public class UserTestDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime TsUpdate { get; set; }
        public string? Author { get; set; }
        public bool IsPublic { get; set; }
    }
}
