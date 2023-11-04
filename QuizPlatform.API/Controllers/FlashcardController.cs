using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Flashcard;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlashcardController : ControllerBase
    {
        private readonly IFlashcardService _flashcardService;
        private readonly IUserContextService _userContextService;

        public FlashcardController(IFlashcardService flashcardService, IUserContextService userContextService)
        {
            _flashcardService = flashcardService;
            _userContextService = userContextService;
        }

        [Authorize]
        [HttpGet("getList")]
        public async Task<ActionResult> GetUserFlashcardsList()
        {
            var userId = _userContextService.UserId;
            if (userId == null)
                return Unauthorized();

            var result = await _flashcardService.GetUserFlashcardsListAsync(userId.Value);

            return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpGet("get/{flashcardSetId:int}")]
        public async Task<ActionResult> GetFlashcardsBySetId(int flashcardSetId)
        {
            var result = await _flashcardService.GetFlashcardItemsByIdAsync(flashcardSetId);
            return result is null ? NotFound() : Ok(result);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult> CreateNewFlashcardsSet(FlashcardsSetDto dto)
        {
            var userId = _userContextService.UserId;
            if (userId == null)
                return Unauthorized();

            var result = await _flashcardService.CreateNewFlashcardsSetAsync(dto, userId.Value);

            return result is not null ? Ok(result) : BadRequest();
        }

        [Authorize]
        [HttpPost("generateFromTest/{testId:int}")]
        public async Task<ActionResult> GenerateFlashcardsSetFromTest(int testId)
        {
            var userId = _userContextService.UserId;
            if (userId == null)
                return Unauthorized();

            var result = await _flashcardService.GenerateFlashcardsSetFromTestAsync(testId, userId.Value);

            return result != null ? Ok(result) : BadRequest();
        }

        [Authorize]
        [HttpPut("edit/{id:int}")]
        public async Task<ActionResult> ModifyFlashcardsSet(FlashcardsSetDto dto, [FromRoute] int id)
        {
            var userId = _userContextService.UserId;
            if (userId == null)
                return Unauthorized();

            var result = await _flashcardService.ModifyFlashcardsSetAsync(dto, id, userId.Value);

            return result ? Ok() : BadRequest();
        }

        [Authorize]
        [HttpDelete("delete/{flashcardsSetId}")]
        public async Task<ActionResult> DeleteFlashcardsSetById(int flashcardsSetId)
        {
            await _flashcardService.DeleteFlashcardsSetByIdAsync(flashcardsSetId);
            return Ok();
        }
    }
}
