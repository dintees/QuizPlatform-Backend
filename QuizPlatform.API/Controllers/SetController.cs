using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SetController : ControllerBase
{
    private readonly ISetService _setService;
    private readonly IUserContextService _userContextService;

    public SetController(ISetService setService, IUserContextService userContextService)
    {
        _setService = setService;
        _userContextService = userContextService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetAllUserSets()
    {
        var userId = _userContextService.UserId;

        if (userId is null) return BadRequest();
        var userSets = await _setService.GetAllUserSets(userId.Value);

        return Ok(userSets);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SetDto?>> GetByIdAsync(int id)
    {
        var set = await _setService.GetByIdAsync(id);
        if (set is null) return NotFound();
        return Ok(set);
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult> CreateSet(CreateSetDto setDto)
    {
        var userId = _userContextService.UserId;
        if (userId is null) return Unauthorized();

        var createdSetResult = await _setService.CreateNewSetAsync(setDto, userId.Value);
        if (createdSetResult.Success) return Ok(createdSetResult);
        return BadRequest(createdSetResult.ErrorMessage);
    }

    [Authorize]
    [HttpPost("createWithQuestions")]
    public async Task<ActionResult> CreateSetWithQuestions(CreateSetDto setDto)
    {
        var userId = _userContextService.UserId;
        if (userId is null) return Unauthorized();

        var result = await _setService.CreateNewSetWithQuestions(setDto, userId.Value);
        return Ok(result);
    }

    [HttpPost("addQuestion/{setId:int}")]
    public async Task<ActionResult> AddQuestionToSet(int setId, [FromBody] int questionId)
    {
        bool edited = await _setService.AddQuestionToSetAsync(setId, questionId);
        
        if (edited) return Ok();
        return BadRequest();
    }

    [HttpPut("edit/{id}")]
    public async Task<ActionResult> EditSetProperties(int id, SetDto setDto)
    {
        var result = await _setService.ModifySetPropertiesAsync(id, setDto);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpDelete("removeQuestion/{setId:int}")]
    public async Task<ActionResult> RemoveQuestionFromSet(int setId, [FromBody] int questionId)
    {
        bool isRemoved = await _setService.RemoveQuestionFromSetAsync(setId, questionId);
        return isRemoved ? Ok() : BadRequest();
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<ActionResult> DeleteSet(int id)
    {
        bool isDeleted = await _setService.DeleteByIdAsync(id);
        return isDeleted ? Ok() : BadRequest();
    }
}