using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.Infrastructure.Services;

public class SetService : ISetService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SetService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<SetDto?> GetByIdAsync(int id)
    {
        var set = await _context.Sets.AsNoTracking()//.AsSplitQuery()
            .Include(e => e.Questions)!.ThenInclude(r => r.Question).ThenInclude(e => e!.Answers)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (set is null) return null;
        
        var setDto = _mapper.Map<SetDto>(set);
        return setDto;
    }

    public async Task<bool> CreateNewSetAsync(CreateSetDto dto)
    {
        var newSet = _mapper.Map<Set>(dto);
        await _context.Sets.AddAsync(newSet);
        bool isCreated = await _context.SaveChangesAsync() > 0;
        return isCreated;
    }

    public async Task<bool> AddQuestionToSetAsync(int setId, int questionId)
    {
        var set = await _context.Sets.FirstOrDefaultAsync(e => e.Id == setId);
        var question = await _context.Questions.FirstOrDefaultAsync(e => e.Id == questionId);
        
        if (set is null || question is null) return false;

        // if (Array.Exists<>(set.Questions, e => e. == question)) return false;

        var questionSet = new QuestionSet
        {
            Question = question,
            Set = set
        };

        try
        {
            await _context.QuestionSets.AddAsync(questionSet);
            bool added = await _context.SaveChangesAsync() > 0;
            return added;
        }
        catch (DbUpdateException)
        {
            return false; 
        }
    }

    public async Task<bool> RemoveQuestionFromSetAsync(int setId, int questionId)
    {
        var question = await _context.QuestionSets.FirstOrDefaultAsync(r => r.QuestionId == questionId && r.SetId == setId);
        if (question is null) return false;
        _context.QuestionSets.Remove(question);
        bool isRemoved = await _context.SaveChangesAsync() > 0;
        return isRemoved;
    }
}