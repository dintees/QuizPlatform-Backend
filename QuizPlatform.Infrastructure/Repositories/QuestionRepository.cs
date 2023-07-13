using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Infrastructure.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public QuestionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            var question = await _context.Questions.AsNoTracking()
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == id);

            return question;
        }

        public async Task<bool> MarkQuestionAsDeleted(int id)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(e => e.Id == id);
            if (question is null) return false;
            question.IsDeleted = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddNewQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ModifyQuestion(int id, Question question, ICollection<QuestionAnswer> answers)
        {
            var questionEntity = await _context.Questions.Include(e => e.Answers).FirstOrDefaultAsync(e => e.Id == id);
            if (questionEntity is null) return false;

            questionEntity.Content = question.Content;
            
            questionEntity.QuestionType = question.QuestionType;
            if (questionEntity.Answers is not null)
                _context.Answers.RemoveRange(questionEntity.Answers);

            questionEntity.Answers = answers;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<QuestionType?> GetQuestionTypeAsync(QuestionTypeName questionTypeName)
        {
            return await _context.QuestionTypes.FirstOrDefaultAsync(e => e.Name == questionTypeName);
        }
    }
}
