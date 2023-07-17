using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface ISetRepository
    {
        Task<Set?> GetSetWithQuestionsByIdAsync(int id, bool readOnly = true);
        Task<Set?> GetSetByIdAsync(int id, bool readOnly = true);
        Task InsertSetAsync(Set set);
        void UpdateSet(Set set);
        Task<bool> SaveAsync();
        Task InsertQuestionSetAsync(QuestionSet questionSet);
        Task<QuestionSet?> GetQuestionSetBySetIdAndQuestionIdAsync(int setId, int questionId);
        void RemoveQuestionFromSet(QuestionSet questionSet);
    }
}