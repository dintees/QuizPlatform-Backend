using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.Infrastructure.Services;

public class SetService : ISetService
{
    private readonly IMapper _mapper;
    private readonly ISetRepository _setRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IValidator<Set> _setValidator;

    public SetService(IMapper mapper, ISetRepository setRepository, IQuestionRepository questionRepository, IValidator<Set> setValidator)
    {
        _mapper = mapper;
        _setRepository = setRepository;
        _questionRepository = questionRepository;
        _setValidator = setValidator;
    }

    public async Task<SetDto?> GetByIdAsync(int id)
    {
        var set = await _setRepository.GetSetWithQuestionsByIdAsync(id);
        if (set is null) return null;

        var setDto = _mapper.Map<SetDto>(set);
        return setDto;
    }

    public async Task<Result<int>> CreateNewSetAsync(CreateSetDto dto)
    {
        var newSet = _mapper.Map<Set>(dto);
        var validationResult = await _setValidator.ValidateAsync(newSet);
        if (!validationResult.IsValid) return new Result<int> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _setRepository.InsertSetAsync(newSet);
        return await _setRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = newSet.Id }
            : new Result<int>() { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<int>> ModifySetPropertiesAsync(int id, SetDto setDto)
    {
        var set = _mapper.Map<Set>(setDto);
        if (set is null)
            return new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
        var validationResult = await _setValidator.ValidateAsync(set);
        if (!validationResult.IsValid)
            return new Result<int>
            { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        var setEntity = await _setRepository.GetSetByIdAsync(id);
        if (setEntity is null)
            return new Result<int> { Success = false, ErrorMessage = SetErrorMessages.NotFound };

        setEntity.Title = set.Title;
        setEntity.Description = set.Description;

        _setRepository.UpdateSet(setEntity);
        return await _setRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = setEntity.Id }
            : new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<bool> AddQuestionToSetAsync(int setId, int questionId)
    {
        var set = await _setRepository.GetSetByIdAsync(setId, false);
        var question = await _questionRepository.GetQuestionByIdAsync(questionId, false);

        if (set is null || question is null) return false;

        // if (Array.Exists<>(set.Questions, e => e. == question)) return false;

        var questionSet = new QuestionSet
        {
            Question = question,
            Set = set
        };

        try
        {
            await _setRepository.InsertQuestionSetAsync(questionSet);
            return await _setRepository.SaveAsync();
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> RemoveQuestionFromSetAsync(int setId, int questionId)
    {
        var questionSet = await _setRepository.GetQuestionSetBySetIdAndQuestionIdAsync(setId, questionId);
        if (questionSet is null) return false;

        _setRepository.RemoveQuestionFromSet(questionSet);
        return await _setRepository.SaveAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var set = await _setRepository.GetSetByIdAsync(id);
        if (set is null) return false;
        set.IsDeleted = true;
        _setRepository.UpdateSet(set);
        return await _setRepository.SaveAsync();
    }
}