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
            var result = await _flashcardService.GetFlashcardItemsById(flashcardSetId);
            return result is null ? NotFound() : Ok(result);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult> CreateNewFlashcardsSet(FlashcardsSetDto dto)
        {
            var userId = _userContextService.UserId;
            if (userId == null)
                return Unauthorized();

            var result = await _flashcardService.CreateNewFlashcardsSet(dto, userId.Value);

            return result is not null ? Ok(result) : BadRequest();
        }

        [Authorize]
        [HttpPut("edit/{id:int}")]
        public async Task<ActionResult> ModifyFlashcardsSet(FlashcardsSetDto dto, [FromRoute] int id)
        {
            var userId = _userContextService.UserId;
            if (userId == null)
                return Unauthorized();

            var result = await _flashcardService.ModifyFlashcardsSet(dto, id, userId.Value);

            return result ? Ok() : BadRequest();
        }
    }
}
