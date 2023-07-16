using AutoMapper;
using FluentValidation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<Question> _questionValidator;

    public QuestionService(IQuestionRepository questionRepository, IMapper mapper, IValidator<Question> questionValidator)
    {
        _questionRepository = questionRepository;
        _mapper = mapper;
        _questionValidator = questionValidator;
    }

    public async Task<QuestionDto?> GetByIdAsync(int id)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id);
        if (question is null) return null;
        var questionDto = _mapper.Map<QuestionDto>(question);
        return questionDto;
    }

    public async Task<Result<int>> CreateQuestionAsync(CreateQuestionDto createQuestionDto)
    {
        var question = _mapper.Map<Question>(createQuestionDto);

        var validationResult = await _questionValidator.ValidateAsync(question);
        if (!validationResult.IsValid) return new Result<int> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _questionRepository.InsertQuestionAsync(question);
        return await _questionRepository.SaveAsync() ? new Result<int> { Success = true, Value = question.Id } : new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<string?> ModifyQuestionAsync(int id, CreateQuestionDto createQuestionDto)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id, false);
        if (question is null) 
            return QuestionErrorMessages.QuestionDoesNotExist;

        var newQuestion = _mapper.Map<Question>(createQuestionDto);

        var validationReuslt = await _questionValidator.ValidateAsync(newQuestion);
        if (!validationReuslt.IsValid) 
            return validationReuslt.Errors.FirstOrDefault()?.ErrorMessage;

        question.Content = createQuestionDto.Question;
        if (question.Answers is not null)
            _questionRepository.DeleteAnswers(question.Answers);
        question.Answers = _mapper.Map<ICollection<QuestionAnswer>>(createQuestionDto.Answers);

        _questionRepository.UpdateQuestion(question);
        return await _questionRepository.SaveAsync() ? null : GeneralErrorMessages.GeneralError;
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