using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var question = await _questionService.GetByIdAsync(id)!;
        if (question is null) return NotFound();
        return Ok(question);
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateQuestion(CreateQuestionDto createQuestionDto)
    {
        bool isCreated = await _questionService.CreateQuestionAsync(createQuestionDto);
        if (!isCreated) return BadRequest();
        
        return Ok();
    }

    [HttpPut("edit/{id:int}")]
    public async Task<ActionResult> ModifyQuestion(int id, CreateQuestionDto createQuestionDto)
    {
        bool isModified = await _questionService.ModifyQuestion(id, createQuestionDto);
        if (isModified == false) return BadRequest();
        return Ok();
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<ActionResult> DeleteQuestion(int id)
    {
        bool isDeleted = await _questionService.DeleteByIdAsync(id);
        return isDeleted ? Ok() : BadRequest();
    }
}