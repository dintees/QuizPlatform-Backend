using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface ITestRepository
    {
        Task<Test?> GetTestWithQuestionsByIdAsync(int id, bool readOnly = true);
        Task<Test?> GetByIdAsync(int id, bool readOnly = true);
        Task<List<Test>?> GetTestsByUserIdAsync(int userId);
        Task AddAsync(Test test);
        void Update(Test test);
        Task<bool> SaveAsync();
        Task<List<Test>?> GetPublicTestsListAsync();
    }
}