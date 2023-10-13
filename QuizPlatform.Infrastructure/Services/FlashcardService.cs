using AutoMapper;
using Microsoft.Identity.Client;
using QuizPlatform.Infrastructure.Entities;
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

        public async Task<FlashcardsSetDto?> GetFlashcardItemsById(int flashcardsId)
        {
            var flashcards = await _flashcardRepository.GetFlashcardsSetByIdAsync(flashcardsId);

            return flashcards == null ? null : _mapper.Map<FlashcardsSetDto>(flashcards);
        }

        public async Task<int?> CreateNewFlashcardsSet(FlashcardsSetDto dto, int userId)
        {
            var flashcardEntity = _mapper.Map<Flashcard>(dto);
            flashcardEntity.UserId = userId;

            await _flashcardRepository.AddAsync(flashcardEntity);

            bool isCreated = await _flashcardRepository.SaveAsync();

            return isCreated ? flashcardEntity.Id : null;
        }

        public async Task<bool> ModifyFlashcardsSet(FlashcardsSetDto dto, int id, int userId)
        {
            var flashcardEntity = await _flashcardRepository.GetFlashcardsSetByIdAsync(id);
            if (flashcardEntity == null || flashcardEntity.FlashcardItems == null) return false;

            var dbFlashcardsToRemove = new List<FlashcardItem>(flashcardEntity.FlashcardItems);

            flashcardEntity.Title = dto.Title;
            flashcardEntity.Description = dto.Description;

            if (dto.FlashcardItems != null)
                foreach (var userFlashcard in dto.FlashcardItems)
                {
                    var found = flashcardEntity.FlashcardItems?.FirstOrDefault(e => e.Id == userFlashcard.Id && e.Id != 0);
                    if (found != null)
                    {
                        found.FirstSide = userFlashcard.FirstSide;
                        found.SecondSide = userFlashcard.SecondSide;
                        dbFlashcardsToRemove.Remove(found);
                    }
                    else
                        flashcardEntity.FlashcardItems?.Add(_mapper.Map<FlashcardItem>(userFlashcard));
                }

            foreach (var flashcard in dbFlashcardsToRemove)
                flashcardEntity.FlashcardItems?.Remove(flashcard);

            _flashcardRepository.Update(flashcardEntity);

            return await _flashcardRepository.SaveAsync();
        }
    }
}
