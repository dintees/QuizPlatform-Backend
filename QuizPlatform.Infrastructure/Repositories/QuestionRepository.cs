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

        public async Task<Question?> GetQuestionByIdAsync(int id, bool readOnly = true)
        {
            if (readOnly) _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var question = await _context.Questions
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == id);

            return question;
        }

        public async Task InsertQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
        }

        public void UpdateQuestion(Question question)
        {
            _context.Entry(question).State = EntityState.Modified;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void DeleteAnswers(ICollection<QuestionAnswer> answers)
        {
            _context.Answers.RemoveRange(answers);
        }
    }
}
