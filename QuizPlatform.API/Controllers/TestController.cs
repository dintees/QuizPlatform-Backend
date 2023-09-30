using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Test;

namespace QuizPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly ITestService _testService;
    private readonly IUserContextService _userContextService;

    public TestController(ITestService testService, IUserContextService userContextService)
    {
        _testService = testService;
        _userContextService = userContextService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetAllUserSets()
    {
        var userId = _userContextService.UserId;

        if (userId is null) return BadRequest();
        var userSets = await _testService.GetAllUserTests(userId.Value);

        return Ok(userSets);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestDto?>> GetByIdAsync(int id)
    {
        var set = await _testService.GetByIdAsync(id);
        if (set is null) return NotFound();
        return Ok(set);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult> CreateSet(CreateTestDto testDto)
    {
        var userId = _userContextService.UserId;
        if (userId is null) return Unauthorized();

        var createdSetResult = await _testService.CreateNewTestAsync(testDto, userId.Value);
        if (createdSetResult.Success) return Ok(createdSetResult);
        return BadRequest(createdSetResult.ErrorMessage);
    }

    [Authorize]
    [HttpPost("createWithQuestions")]
    public async Task<ActionResult> CreateSetWithQuestions(CreateTestDto testDto)
    {
        var userId = _userContextService.UserId;
        if (userId is null) return Unauthorized();

        var result = await _testService.CreateNewTestWithQuestionsAsync(testDto, userId.Value);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("addQuestion/{setId:int}")]
    public async Task<ActionResult> AddQuestionToSet(int setId, [FromBody] int questionId)
    {
        bool edited = await _testService.AddQuestionToTestAsync(setId, questionId);

        if (edited) return Ok();
        return BadRequest();
    }

    [Authorize]
    [HttpPut("edit/{id:int}")]
    public async Task<ActionResult> EditSetProperties(int id, TestDto testDto)
    {
        var result = await _testService.ModifyTestAsync(id, testDto);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [Authorize]
    [HttpPost("duplicate/{setId:int}")]
    public async Task<ActionResult> DuplicateSet(int setId)
    {
        var userId = _userContextService.UserId;
        if (userId is null) return Unauthorized();

        var result = await _testService.DuplicateTestAsync(setId, userId.Value);
        if (result.Success) return Ok(result);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("removeQuestion/{setId:int}")]
    public async Task<ActionResult> RemoveQuestionFromSet(int setId, [FromBody] int questionId)
    {
        bool isRemoved = await _testService.RemoveQuestionFromTestAsync(setId, questionId);
        return isRemoved ? Ok() : BadRequest();
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<ActionResult> DeleteSet(int id)
    {
        bool isDeleted = await _testService.DeleteByIdAsync(id);
        return isDeleted ? Ok() : BadRequest();
    }
}