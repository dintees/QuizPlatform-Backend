using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IQuestionService
{
    Task<QuestionDto?> GetByIdAsync(int id);
    Task<Result<int>> CreateQuestionAsync(CreateQuestionDto dto);
    Task<string?> ModifyQuestionAsync(int id, CreateQuestionDto createQuestionDto);
    Task<bool> DeleteByIdAsync(int id);
}