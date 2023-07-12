using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Set;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SetController : ControllerBase
{
    private readonly ISetService _setService;

    public SetController(ISetService setService, IMapper mapper)
    {
        _setService = setService;
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SetDto?>> GetByIdAsync(int id)
    {
        var set = await _setService.GetByIdAsync(id);
        if (set is null) return NotFound();
        return Ok(set);
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateSet(CreateSetDto setDto)
    {
        bool isCreated = await _setService.CreateNewSetAsync(setDto);
        return isCreated ? Ok() : BadRequest();
    }

    [HttpPost("addQuestion/{setId:int}")]
    public async Task<ActionResult> AddQuestionToSet(int setId, [FromBody] int questionId)
    {
        bool edited = await _setService.AddQuestionToSetAsync(setId, questionId);
        
        if (edited) return Ok();
        return BadRequest();
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