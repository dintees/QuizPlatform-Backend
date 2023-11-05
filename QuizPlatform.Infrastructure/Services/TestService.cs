using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.ErrorMessages;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Models.TestSession;

namespace QuizPlatform.Infrastructure.Services;

public class TestService : ITestService
{
    private readonly IMapper _mapper;
    private readonly ITestRepository _testRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ITestSessionRepository _testSessionRepository;
    private readonly IUserAnswersRepository _userAnswersRepository;
    private readonly IValidator<Test> _testValidator;

    public TestService(IMapper mapper, ITestRepository testRepository, IQuestionRepository questionRepository, ITestSessionRepository testSessionRepository, IUserAnswersRepository userAnswersRepository, IValidator<Test> testValidator)
    {
        _mapper = mapper;
        _testRepository = testRepository;
        _questionRepository = questionRepository;
        _testSessionRepository = testSessionRepository;
        _userAnswersRepository = userAnswersRepository;
        _testValidator = testValidator;
    }

    public async Task<List<UserTestDto>?> GetAllUserTestsAsync(int? userId)
    {
        if (userId is null)
        {
            // all users - for admin
            return _mapper.Map<List<UserTestDto>?>(await _testRepository.GetTestsByUserIdAsync(null));
        }
        // for single user
        return _mapper.Map<List<UserTestDto>?>(await _testRepository.GetTestsByUserIdAsync(userId.Value));
    }

    public async Task<TestDto?> GetByIdAsync(int id, int? userId)
    {
        var test = await _testRepository.GetTestWithQuestionsByIdAsync(id);
        if (test is null) return null;
        if (userId is not null && test.UserId != userId) return null;

        test.Questions = test.Questions.Where(q => !q.IsDeleted).ToList();

        var testDto = _mapper.Map<TestDto>(test);
        return testDto;
    }

    public async Task<Result<int>> CreateNewTestAsync(CreateTestDto dto, int userId)
    {
        var newTest = _mapper.Map<Test>(dto);
        newTest.UserId = userId;

        var validationResult = await _testValidator.ValidateAsync(newTest);
        if (!validationResult.IsValid) return new Result<int> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _testRepository.AddAsync(newTest);
        return await _testRepository.SaveAsync()
            ? new Result<int> { Success = true, Value = newTest.Id }
            : new Result<int>() { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<TestDto>> ModifyTestAsync(int testId, TestDto dto, int? userId)
    {
        var test = await _testRepository.GetTestWithQuestionsByIdAsync(testId, false, true);
        if (test is null)
            return new Result<TestDto> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        // If userId is null -> admin access. Otherwise only author can modify the test
        if (userId is not null && test.UserId != userId)
            return new Result<TestDto> { Success = false, ErrorMessage = GeneralErrorMessages.Unauthorized };

        var validationResult = await _testValidator.ValidateAsync(_mapper.Map<Test>(dto));
        if (!validationResult.IsValid)
            return new Result<TestDto> { Success = false, ErrorMessage = TestErrorMessages.ValidationError };
        //return new Result<TestDto> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        var deletedQuestions = test.Questions.Where(e => e.IsDeleted);

        test.Title = dto.Title;
        test.Description = dto.Description;
        test.IsPublic = dto.IsPublic;
        test.ShuffleQuestions = dto.ShuffleQuestions;
        test.ShuffleAnswers = dto.ShuffleAnswers;
        test.OneQuestionMode = dto.OneQuestionMode;

        test.Questions = test.Questions.Where(e => !e.IsDeleted).ToList();

        var questionsEntity = test.Questions;

        if (dto.Questions is not null)
            foreach (var question in dto.Questions)
            {
                var foundEntity = questionsEntity?.FirstOrDefault(e => e.Id == question.Id && e.Id != 0);
                if (foundEntity is null)
                {
                    var newQuestion = _mapper.Map<Question>(question);
                    newQuestion.TestId = testId;
                    test.Questions.Add(newQuestion);
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

        // Update score results for existing tests in the database
        var connectedTestSessions = await _testSessionRepository.GetByTestIdAsync(testId);
        if (connectedTestSessions is not null)
        {
            foreach (var testSession in connectedTestSessions)
            {
                testSession.Test = test;
                testSession.Test.Questions = testSession.Test.Questions.Concat(deletedQuestions).ToList();
                var userAnswers = await _userAnswersRepository.GetUserAnswersByTestSessionIdAsync(testSession.Id);

                var userAnswersDto = new List<UserAnswersDto>();
                if (userAnswers is not null)
                {
                    foreach (var userAnswer in userAnswers)
                    {
                        var found = userAnswersDto.Find(e => e.QuestionId == userAnswer.QuestionId);
                        if (found is null)
                            userAnswersDto.Add(new UserAnswersDto { QuestionId = userAnswer.QuestionId, AnswerIds = userAnswer.QuestionAnswerId is null ? null : new List<int> { userAnswer.QuestionAnswerId!.Value }, ShortAnswerValue = userAnswer.ShortAnswerValue });
                        else
                            found.AnswerIds?.Add(userAnswer.QuestionAnswerId!.Value);
                    }
                }
                int score = TestSessionService.GetScoreResult(userAnswersDto, testSession);
                testSession.Score = score;
            }
        }

        var modified = await _testRepository.SaveAsync();
        test.Questions = test.Questions.Where(e => !e.IsDeleted).ToList();
        return modified ? new Result<TestDto> { Success = true, Value = _mapper.Map<TestDto>(test) } : new Result<TestDto> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<Result<int>> DuplicateTestAsync(int testId, int userId)
    {
        var test = await _testRepository.GetTestWithQuestionsByIdAsync(testId);
        if (test is null)
            return new Result<int> { Success = false, ErrorMessage = TestErrorMessages.NotFound };

        var newSet = new Test()
        {
            Title = string.Concat(test.Title, " - Copy"),
            Description = test.Description,
            Questions = test.Questions.Select(q => new Question { Content = q.Content, MathMode = q.MathMode, QuestionType = q.QuestionType, Answers = q.Answers?.Select(a => new QuestionAnswer { Content = a.Content, Correct = a.Correct }).ToList() }).ToList(),
            UserId = userId,
            ShuffleQuestions = test.ShuffleQuestions,
            ShuffleAnswers = test.ShuffleAnswers,
            OneQuestionMode = test.OneQuestionMode,
        };

        await _testRepository.AddAsync(newSet);
        return await _testRepository.SaveAsync() ?
                new Result<int> { Success = true, Value = newSet.Id } :
                new Result<int> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }

    public async Task<List<UserTestDto>?> GetAllPublicTestsListAsync()
    {
        var tests = await _testRepository.GetPublicTestsListAsync();

        return _mapper.Map<List<UserTestDto>?>(tests);
    }

    public async Task<List<TestDto>?> GetAllUserTestsWithQuestionsContentAsync(int userId)
    {
        var userTests = await _testRepository.GetTestsByUserIdAsync(userId, true);
        if (userTests is null) return null;

        var tests = new List<TestDto>();

        foreach (var test in userTests.Select(userTest => _mapper.Map<TestDto>(userTest)))
        {
            test.Questions = test.Questions?.Where(e => !e.IsDeleted).ToList();
            tests.Add(test);
        }

        return tests;
    }

    public async Task<bool> AddQuestionToTestAsync(int setId, int questionId)
    {
        var test = await _testRepository.GetByIdAsync(setId, false);
        var question = await _questionRepository.GetQuestionByIdAsync(questionId, false);

        if (test is null || question is null) return false;

        try
        {
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

        var question = test.Questions.FirstOrDefault(x => x.Id == questionId);
        if (question is null) return false;

        test.Questions.Remove(question);
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

        var validationResult = await _testValidator.ValidateAsync(test);

        if (!validationResult.IsValid)
            return new Result<TestDto> { Success = false, ErrorMessage = TestErrorMessages.ValidationError };
        //return new Result<TestDto> { Success = false, ErrorMessage = validationResult.Errors.FirstOrDefault()?.ErrorMessage };

        await _testRepository.AddAsync(test);
        var created = await _testRepository.SaveAsync();

        return created ? new Result<TestDto> { Success = true, Value = _mapper.Map<TestDto>(test) } : new Result<TestDto> { Success = false, ErrorMessage = GeneralErrorMessages.GeneralError };
    }
}