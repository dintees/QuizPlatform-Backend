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
        var result = await _questionService.CreateQuestionAsync(createQuestionDto);
        if (result is null)
        {
            return Ok();
        }

        return BadRequest(result);
    }

    [HttpPut("edit/{id:int}")]
    public async Task<ActionResult> ModifyQuestion(int id, CreateQuestionDto createQuestionDto)
    {
        var modificationResult = await _questionService.ModifyQuestionAsync(id, createQuestionDto);
        if (modificationResult is null)
            return Ok();
        return BadRequest(modificationResult);
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<ActionResult> DeleteQuestion(int id)
    {
        bool isDeleted = await _questionService.DeleteByIdAsync(id);
        return isDeleted ? Ok() : BadRequest();
    }
}