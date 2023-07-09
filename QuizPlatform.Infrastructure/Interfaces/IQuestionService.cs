using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Interfaces;

public interface IQuestionService
{
    Task<CreateQuestionDto?>? GetByIdAsync(int id);
    Task<bool> CreateQuestionAsync(CreateQuestionDto dto);
    Task<bool> ModifyQuestion(int id, CreateQuestionDto createQuestionDto);
    Task<bool> DeleteByIdAsync(int id);
}