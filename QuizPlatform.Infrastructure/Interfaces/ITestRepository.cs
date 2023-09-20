using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface ITestRepository
    {
        Task<Test?> GetSetWithQuestionsByIdAsync(int id, bool readOnly = true);
        Task<Test?> GetSetByIdAsync(int id, bool readOnly = true);
        Task<List<Test>?> GetSetsByUserIdAsync(int userId);
        Task InsertSetAsync(Test test);
        void UpdateSet(Test test);
        Task<bool> SaveAsync();
    }
}