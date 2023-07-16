using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetQuestionByIdAsync(int id, bool readOnly = true);
        Task InsertQuestionAsync(Question question);
        Task<bool> SaveAsync();
        void UpdateQuestion(Question question);
        void DeleteAnswers(ICollection<QuestionAnswer> answers);
    }
}