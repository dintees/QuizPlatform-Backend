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
    private readonly ITestRepository _testRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IValidator<Test> _setValidator;

    public TestService(IMapper mapper, ITestRepository testRepository, IQuestionRepository questionRepository, IValidator<Test> setValidator)
    {
        _mapper = mapper;
        _testRepository = testRepository;
        _questionRepository = questionRepository;
        _setValidator = setValidator;
    }

    public async Task<List<UserTestDto>?> GetAllUserTests(int userId)
    {
        var sets = await _testRepository.GetTestsByUserIdAsync(userId);

        return _mapper.Map<List<UserTestDto>?>(sets);
    }

    public async Task<TestDto?> GetByIdAsync(int id)
    {
        var set = await _testRepository.GetTestWithQuestionsByIdAsync(id);
        if (set is null) return null;
        set.Questions = set.Questions.Where(q => !q.IsDeleted).ToList();

        var setDto = _mapper.Map<TestDto>(set);
        return setDto;
    }

    public async Task<Result<int>> CreateNewTestAsync(CreateTestDto dto, int userId)
    {
        var newSet = _mapper.Map<Test>(dto);
        newSet.UserId = userId;

        var validationResult = await _setValidator.ValidateAsync(newSet);
        if (!validationResult.IsValid) return new Result<int> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _testRepository.AddAsync(newSet);
        return await _testRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = newSet.Id }
            : new Result<int>() { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<int>> ModifyTestPropertiesAsync(int id, TestDto testDto)
    {
        var set = _mapper.Map<Test>(testDto);
        if (set is null)
            return new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
        var validationResult = await _setValidator.ValidateAsync(set);
        if (!validationResult.IsValid)
            return new Result<int>
            { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        var setEntity = await _testRepository.GetByIdAsync(id);
        if (setEntity is null)
            return new Result<int> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        setEntity.Title = set.Title;
        setEntity.Description = set.Description;

        _testRepository.Update(setEntity);
        return await _testRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = setEntity.Id }
            : new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<TestDto>> ModifyTestAsync(int id, TestDto dto)
    {
        var test = await _testRepository.GetTestWithQuestionsByIdAsync(id, false);
        if (test is null)
            return new Result<TestDto> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        var validationResult = await _setValidator.ValidateAsync(_mapper.Map<Test>(dto));
        if (!validationResult.IsValid)
            return new Result<TestDto> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        test.Title = dto.Title;
        test.Description = dto.Description;
        test.IsPublic = dto.IsPublic;

        var questionsEntity = test.Questions;

        if (dto.Questions is not null)
            foreach (var question in dto.Questions)
            {
                var foundEntity = questionsEntity?.FirstOrDefault(e => e.Id == question.Id);
                if (foundEntity is null)
                {
                    var newQuestion = _mapper.Map<Question>(question);
                    newQuestion.TestId = id;
                    await _questionRepository.InsertQuestionAsync(newQuestion);
                }
                else
                {
                    foundEntity.Content = question.Question;
                    foundEntity.MathMode = question.MathMode;
                    foundEntity.IsDeleted = question.IsDeleted;

                    var foundAnswerEntities = foundEntity.Answers?.ToList();
                    if (question.Answers is null) continue;
                    foreach (var answer in question.Answers)
                    {
                        var foundAnswer = foundAnswerEntities?.Find(e => e.Id == answer.Id);
                        if (foundAnswer is null)
                            foundEntity.Answers?.Add(new QuestionAnswer { Content = answer.Answer, Correct = answer.Correct });
                        else
                        {
                            foundAnswer.Content = answer.Answer;
                            foundAnswer.Correct = answer.Correct;
                            foundAnswerEntities?.Remove(foundAnswer);
                        }
                    }
                    if (foundAnswerEntities is not null)
                        _questionRepository.DeleteAnswers(foundAnswerEntities);
                }
            }


        var modified = await _testRepository.SaveAsync();
        return modified ? new Result<TestDto> { Success = true, Value = _mapper.Map<TestDto>(test) } : new Result<TestDto> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<int>> DuplicateTestAsync(int setId, int userId)
    {
        var set = await _testRepository.GetTestWithQuestionsByIdAsync(setId);
        if (set is null)
            return new Result<int> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        var newSet = new Test()
        {
            Title = string.Concat(set.Title, " - Copy"),
            Description = set.Description,
            Questions = set.Questions.Select(q => new Question { Content = q.Content, MathMode = q.MathMode, QuestionType = q.QuestionType, Answers = q.Answers?.Select(a => new QuestionAnswer { Content = a.Content, Correct = a.Correct }).ToList() }).ToList(),
            UserId = userId
        };

        await _testRepository.AddAsync(newSet);
        return await _testRepository.SaveAsync() ?
                new Result<int> { Success = true, Value = newSet.Id } :
                new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<bool> AddQuestionToTestAsync(int setId, int questionId)
    {
        var test = await _testRepository.GetByIdAsync(setId, false);
        var question = await _questionRepository.GetQuestionByIdAsync(questionId, false);

        if (test is null || question is null) return false;

        try
        {
            test.Questions ??= new List<Question>();
            test.Questions.Add(question);
            return await _testRepository.SaveAsync();
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> RemoveQuestionFromTestAsync(int setId, int questionId)
    {
        var test = await _testRepository.GetByIdAsync(setId);
        if (test is null) return false;

        var question = test.Questions?.FirstOrDefault(x => x.Id == questionId);
        if (question is null) return false;

        test.Questions?.Remove(question);
        return await _testRepository.SaveAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var test = await _testRepository.GetTestWithQuestionsByIdAsync(id, false);
        if (test is null) return false;
        test.IsDeleted = true;

        _testRepository.Update(test);
        return await _testRepository.SaveAsync();
    }

    public async Task<Result<TestDto>> CreateNewTestWithQuestionsAsync(CreateTestDto dto, int userId)
    {
        var test = _mapper.Map<Test>(dto);
        test.UserId = userId;

        var validationResult = await _setValidator.ValidateAsync(test);

        if (!validationResult.IsValid)
            return new Result<TestDto> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _testRepository.AddAsync(test);
        var created = await _testRepository.SaveAsync();

        return created ? new Result<TestDto> { Success = true, Value = _mapper.Map<TestDto>(test) } : new Result<TestDto> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }
}