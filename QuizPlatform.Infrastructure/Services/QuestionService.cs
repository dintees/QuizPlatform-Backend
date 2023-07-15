using AutoMapper;
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
        await _questionRepository.InsertQuestionAsync(question);
        return await _questionRepository.SaveAsync();
    }

    public async Task<bool> ModifyQuestion(int id, CreateQuestionDto createQuestionDto)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id, false);
        if (question is null) return false;

        question.Content = createQuestionDto.Question;
        question.QuestionType = await _questionRepository.GetQuestionTypeAsync(createQuestionDto.QuestionType);
        if (question.Answers is not null)
            _questionRepository.DeleteAnswers(question.Answers);
        question.Answers = _mapper.Map<ICollection<QuestionAnswer>>(createQuestionDto.Answers);

        _questionRepository.UpdateQuestion(question);
        return await _questionRepository.SaveAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id);
        if (question is null) return false;
        question.IsDeleted = true;
        _questionRepository.UpdateQuestion(question);

        return await _questionRepository.SaveAsync();
    }
}