using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public QuestionService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<QuestionDto?> GetByIdAsync(int id)
    {
        var question = await _context.Questions.AsNoTracking()
            .Include(e => e.Answers)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (question is null) return null;
        var questionDto = _mapper.Map<QuestionDto>(question);
        return questionDto;
    }

    public async Task<bool> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
    {
        var question = _mapper.Map<Question>(createQuestionDto);
        var questionType = await _context.QuestionTypes.FirstOrDefaultAsync(e => e.Name == createQuestionDto.QuestionType);
        if (questionType is null) return false;
        question.QuestionType = questionType;
        await _context.Questions.AddAsync(question);
        var created = await _context.SaveChangesAsync() > 0;
        return created;
    }

    public async Task<bool> ModifyQuestion(int id, CreateQuestionDto createQuestionDto)
    {
        var question = await _context.Questions.Include(e => e.Answers).FirstOrDefaultAsync(e => e.Id == id);
        if (question is null) return false;

        question.Content = createQuestionDto.Question;
        var questionType = await _context.QuestionTypes.FirstOrDefaultAsync(e => e.Name == createQuestionDto.QuestionType);
        if (questionType is null) return false;
        question.QuestionType = questionType;
        if (question.Answers is not null) 
             _context.Answers.RemoveRange(question.Answers);

        question.Answers = _mapper.Map<ICollection<QuestionAnswer>>(createQuestionDto.Answers);

        bool isModified = await _context.SaveChangesAsync() > 0;
        
        return isModified;
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var question = await _context.Questions.FirstOrDefaultAsync(e => e.Id == id);
        if (question is null) return false;
        question.IsDeleted = true;
        bool isDeleted = await _context.SaveChangesAsync() > 0;
        return true;
    }
}