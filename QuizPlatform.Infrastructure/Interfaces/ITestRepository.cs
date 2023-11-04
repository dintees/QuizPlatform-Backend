using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface ITestRepository
    {
        Task<Test?> GetTestWithQuestionsByIdAsync(int id, bool readOnly = true, bool includeDeleted = false);
        Task<Test?> GetByIdAsync(int id, bool readOnly = true);
        Task<List<Test>?> GetTestsByUserIdAsync(int? userId, bool includeQuestionsWithAnswers = false);
        Task AddAsync(Test test);
        void Update(Test test);
        Task<bool> SaveAsync();
        Task<List<Test>?> GetPublicTestsListAsync();
    }
}