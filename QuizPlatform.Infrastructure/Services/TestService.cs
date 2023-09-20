using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Test;

namespace QuizPlatform.Infrastructure.Services;

public class TestService : ITestService
{
    private readonly IMapper _mapper;
    private readonly ITestRepository _setRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IValidator<Test> _setValidator;

    public TestService(IMapper mapper, ITestRepository setRepository, IQuestionRepository questionRepository, IValidator<Test> setValidator)
    {
        _mapper = mapper;
        _setRepository = setRepository;
        _questionRepository = questionRepository;
        _setValidator = setValidator;
    }

    public async Task<List<UserTestDto>?> GetAllUserSets(int userId)
    {
        var sets = await _setRepository.GetSetsByUserIdAsync(userId);

        return _mapper.Map<List<UserTestDto>?>(sets);
    }

    public async Task<TestDto?> GetByIdAsync(int id)
    {
        var set = await _setRepository.GetSetWithQuestionsByIdAsync(id);
        if (set is null) return null;
        set.Questions = set.Questions?.Where(q => !q.IsDeleted).ToList();

        var setDto = _mapper.Map<TestDto>(set);
        return setDto;
    }

    public async Task<Result<int>> CreateNewSetAsync(CreateTestDto dto, int userId)
    {
        var newSet = _mapper.Map<Test>(dto);
        newSet.UserId = userId;

        var validationResult = await _setValidator.ValidateAsync(newSet);
        if (!validationResult.IsValid) return new Result<int> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _setRepository.InsertSetAsync(newSet);
        return await _setRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = newSet.Id }
            : new Result<int>() { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<int>> ModifySetPropertiesAsync(int id, TestDto testDto)
    {
        var set = _mapper.Map<Test>(testDto);
        if (set is null)
            return new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
        var validationResult = await _setValidator.ValidateAsync(set);
        if (!validationResult.IsValid)
            return new Result<int>
            { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        var setEntity = await _setRepository.GetSetByIdAsync(id);
        if (setEntity is null)
            return new Result<int> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        setEntity.Title = set.Title;
        setEntity.Description = set.Description;

        _setRepository.UpdateSet(setEntity);
        return await _setRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = setEntity.Id }
            : new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<TestDto>> ModifySet(int id, CreateTestDto dto)
    {
        var set = await _setRepository.GetSetWithQuestionsByIdAsync(id, false);
        if (set is null)
            return new Result<TestDto> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        var validationResult = await _setValidator.ValidateAsync(_mapper.Map<Test>(dto));
        if (!validationResult.IsValid)
            return new Result<TestDto> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        set.Title = dto.Title;
        set.Description = dto.Description;
        set.Questions = new List<Question>(_mapper.Map<IEnumerable<Question>>(dto.Questions));

        var modified = await _setRepository.SaveAsync();
        return modified ? new Result<TestDto> { Success = true, Value = _mapper.Map<TestDto>(set) } : new Result<TestDto> { Success = false, ErrorMessage = "Something went wrong" };
    }

    public async Task<Result<int>> DuplicateSetAsync(int setId, int userId)
    {
        var set = await _setRepository.GetSetWithQuestionsByIdAsync(setId);
        if (set is null)
            return new Result<int> { Success = false, ErrorMessage = TestErrorMessages.NotFound};

        var newSet = new Test()
        {
            Title = string.Concat(set.Title, " - Copy"),
            Description = set.Description,
            Questions = set.Questions?.Select(q => new Question { Content = q.Content, MathMode = q.MathMode, QuestionType = q.QuestionType, Answers = q.Answers?.Select(a => new QuestionAnswer { Content = a.Content, Correct = a.Correct}).ToList()}).ToList(),
            UserId = userId
        };

        await _setRepository.InsertSetAsync(newSet);
        return await _setRepository.SaveAsync() ?
                new Result<int> { Success = true, Value = newSet.Id} : 
                new Result<int> { Success = false, ErrorMessage = "Something went wrong." };
    }

    public async Task<bool> AddQuestionToSetAsync(int setId, int questionId)
    {
        var set = await _setRepository.GetSetByIdAsync(setId, false);
        var question = await _questionRepository.GetQuestionByIdAsync(questionId, false);

        if (set is null || question is null) return false;


        try
        {
            set.Questions ??= new List<Question>();
            set.Questions.Add(question);
            return await _setRepository.SaveAsync();
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> RemoveQuestionFromSetAsync(int setId, int questionId)
    {
        var set = await _setRepository.GetSetByIdAsync(setId);
        if (set is null) return false;

        var question = set.Questions?.FirstOrDefault(x => x.Id == questionId);
        if (question is null) return false;

        set.Questions?.Remove(question);
        return await _setRepository.SaveAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var set = await _setRepository.GetSetWithQuestionsByIdAsync(id, false);
        if (set is null) return false;
        set.IsDeleted = true;

        if (set.Questions is not null)
            foreach (var question in set.Questions)
                question.IsDeleted = true;

        _setRepository.UpdateSet(set);
        return await _setRepository.SaveAsync();
    }

    public async Task<Result<TestDto>> CreateNewSetWithQuestionsAsync(CreateTestDto dto, int userId)
    {
        var set = _mapper.Map<Test>(dto);
        set.UserId = userId;

        var validationResult = await _setValidator.ValidateAsync(set);

        if (!validationResult.IsValid)
            return new Result<TestDto> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _setRepository.InsertSetAsync(set);
        var created = await _setRepository.SaveAsync();

        return created ? new Result<TestDto> { Success = true, Value = _mapper.Map<TestDto>(set) } : new Result<TestDto> { Success = false, ErrorMessage = "Something went wrong" };
    }
}