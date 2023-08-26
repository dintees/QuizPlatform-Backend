using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface ISetRepository
    {
        Task<Set?> GetSetWithQuestionsByIdAsync(int id, bool readOnly = true);
        Task<Set?> GetSetByIdAsync(int id, bool readOnly = true);
        Task<List<Set>?> GetSetsByUserIdAsync(int userId);
        Task InsertSetAsync(Set set);
        void UpdateSet(Set set);
        Task<bool> SaveAsync();
    }
}