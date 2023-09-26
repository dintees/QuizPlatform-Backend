using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Models.TestSession;

namespace QuizPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestSessionController : ControllerBase
    {
        private readonly ITestSessionService _testSessionService;
        private readonly IUserContextService _userContextService;
        private readonly IEmailService _emailService;

        public TestSessionController(ITestSessionService testSessionService, IUserContextService userContextService, IEmailService emailService)
        {
            _testSessionService = testSessionService;
            _userContextService = userContextService;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetActiveUserSessions()
        {
            var userId = _userContextService.UserId;
            if (userId is null)
                return Unauthorized();

            var result = await _testSessionService.GetActiveUserTestSessionsAsync(userId.Value);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("get/{testSessionId:int}")]
        public async Task<ActionResult> GetTestSession(int testSessionId)
        {
            var result = await _testSessionService.GetTestByTestSessionIdAsync(testSessionId);
            
            return result.Success ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult> CreateTestSession(CreateTestSessionDto dto)
        {
            var userId = _userContextService.UserId;
            if (userId is null)
                return Unauthorized();

            var result = await _testSessionService.CreateTestSession(dto, userId.Value);

            if (result.Success)
                return Ok(result.Value);

            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("saveAnswers")]
        public async Task<ActionResult> SaveAnswers(List<QuestionDto> dto)
        {
            // TODO
            return NotFound("TODO");
        }
    }
}
