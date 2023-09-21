using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Test;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ITestService
{
    Task<TestDto?> GetByIdAsync(int id);
    Task<List<UserTestDto>?> GetAllUserSets(int userId);
    Task<Result<int>> CreateNewSetAsync(CreateTestDto dto, int userId);
    Task<Result<int>> ModifySetPropertiesAsync(int id,TestDto testDto);
    Task<bool> AddQuestionToSetAsync(int setId, int questionId);
    Task<bool> RemoveQuestionFromSetAsync(int setId, int questionId);
    Task<bool> DeleteByIdAsync(int id);
    Task<Result<TestDto>> CreateNewSetWithQuestionsAsync(CreateTestDto dto, int userId);
    Task<Result<TestDto>> ModifySet(int id, CreateTestDto dto);
    Task<Result<int>> DuplicateSetAsync(int setId, int userId);
    Task<Result<TestDto>> CreateTestSession(CreateTestSessionDto dto);
}
