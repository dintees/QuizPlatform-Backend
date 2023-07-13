using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface IQuestionRepository
    {
        Task<Question?> GetQuestionByIdAsync(int id);
        Task<bool> MarkQuestionAsDeleted(int id);
        Task<bool> AddNewQuestionAsync(Question question);
        Task <QuestionType?> GetQuestionTypeAsync(QuestionTypeName questionTypeName);
        Task<bool> ModifyQuestion(int id, Question question, ICollection<QuestionAnswer> answers);
    }
}