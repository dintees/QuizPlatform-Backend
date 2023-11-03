using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Test;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ITestService
{
    Task<TestDto?> GetByIdAsync(int id, int? userId);
    Task<List<UserTestDto>?> GetAllUserTestsAsync(int? userId);
    Task<Result<int>> CreateNewTestAsync(CreateTestDto dto, int userId);
    Task<Result<int>> ModifyTestPropertiesAsync(int id,TestDto testDto);
    Task<bool> AddQuestionToTestAsync(int setId, int questionId);
    Task<bool> RemoveQuestionFromTestAsync(int setId, int questionId);
    Task<bool> DeleteByIdAsync(int id);
    Task<Result<TestDto>> CreateNewTestWithQuestionsAsync(CreateTestDto dto, int userId);
    Task<Result<TestDto>> ModifyTestAsync(int testId, TestDto dto, int? userId);
    Task<Result<int>> DuplicateTestAsync(int setId, int userId);
    Task<List<UserTestDto>?> GetAllPublicTestsListAsync();
}
