namespace QuizPlatform.Infrastructure.Models
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Value { get; set; }
    }
}
