using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface ISetService
{
    Task<SetDto?> GetByIdAsync(int id);
    Task<Result<int>> CreateNewSetAsync(CreateSetDto dto);
    Task<bool> AddQuestionToSetAsync(int setId, int questionId);
    Task<bool> RemoveQuestionFromSetAsync(int setId, int questionId);
    Task<bool> DeleteByIdAsync(int id);
}
