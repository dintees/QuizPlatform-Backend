using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IMapper _mapper;

    public QuestionService(IQuestionRepository questionRepository, IMapper mapper)
    {
        _questionRepository = questionRepository;
        _mapper = mapper;
    }

    public async Task<QuestionDto?> GetByIdAsync(int id)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id);
        if (question is null) return null;
        var questionDto = _mapper.Map<QuestionDto>(question);
        return questionDto;
    }

    public async Task<bool> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
    {
        var question = _mapper.Map<Question>(createQuestionDto);
        question.QuestionType = await _questionRepository.GetQuestionTypeAsync(createQuestionDto.QuestionType);
        return await _questionRepository.AddNewQuestionAsync(question);
    }

    public async Task<bool> ModifyQuestion(int id, CreateQuestionDto createQuestionDto)
    {
        var editedQuestion = _mapper.Map<Question>(createQuestionDto);
        editedQuestion.QuestionType = await _questionRepository.GetQuestionTypeAsync(createQuestionDto.QuestionType);

       return await _questionRepository.ModifyQuestion(id, editedQuestion, _mapper.Map<ICollection<QuestionAnswer>>(createQuestionDto.Answers));
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        return await _questionRepository.MarkQuestionAsDeleted(id);
    }
}