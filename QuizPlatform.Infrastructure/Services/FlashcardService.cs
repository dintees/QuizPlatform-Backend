using AutoMapper;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Models;
using QuizPlatform.Infrastructure.Models.Flashcard;

namespace QuizPlatform.Infrastructure.Services
{
    public class FlashcardService : IFlashcardService
    {
        private readonly IFlashcardRepository _flashcardRepository;
        private readonly IMapper _mapper;

        public FlashcardService(IFlashcardRepository flashcardRepository, IMapper mapper)
        {
            _flashcardRepository = flashcardRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<UserFlashcardDto>>> GetUserFlashcardsListAsync(int userId)
        {
            var flashcardList = await _flashcardRepository.GetFlashcardByUserId(userId);

            var flashcardListDto = _mapper.Map<List<UserFlashcardDto>>(flashcardList);

            return new Result<List<UserFlashcardDto>> { Success = true, Value = flashcardListDto };
        }

        public async Task<List<FlashcardItemDto>?> GetFlashcardItemsById(int flashcardsId)
        {
            var flashcardItems = await _flashcardRepository.GetFlashcardItemsById(flashcardsId);
            return flashcardItems == null ? null : _mapper.Map<List<FlashcardItemDto>>(flashcardItems);
        }
    }
}
