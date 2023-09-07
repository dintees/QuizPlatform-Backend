using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ISetService
{
    Task<SetDto?> GetByIdAsync(int id);
    Task<List<UserSetDto>?> GetAllUserSets(int userId);
    Task<Result<int>> CreateNewSetAsync(CreateSetDto dto, int userId);
    Task<Result<int>> ModifySetPropertiesAsync(int id,SetDto setDto);
    Task<bool> AddQuestionToSetAsync(int setId, int questionId);
    Task<bool> RemoveQuestionFromSetAsync(int setId, int questionId);
    Task<bool> DeleteByIdAsync(int id);
    Task<Result<SetDto>> CreateNewSetWithQuestionsAsync(CreateSetDto dto, int userId);
}
