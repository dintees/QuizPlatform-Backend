using AutoMapper;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Models.TestSession;
using QuizPlatform.Infrastructure.Models.Flashcard;

namespace QuizPlatform.Infrastructure.Profiles;

public class MainProfile : Profile
{
    public MainProfile()
    {
        // User
        CreateMap<UserRegisterDto, User>();
        CreateMap<User, UserDto>()
            .ForMember(m => m.Role, o => o.MapFrom(s => s.Role!.Name));
        CreateMap<ForgotPasswordDto, ChangeUserPasswordDto>();

        // User sessions
        CreateMap<UserSession, UserSessionDto>()
            .ForMember(m => m.Username, o => o.MapFrom(s => s.User!.UserName));

        // Question
        CreateMap<CreateQuestionDto, Question>()
            .ForMember(m => m.Content, c => c.MapFrom(s => s.Question));

        CreateMap<CreateAnswerDto, QuestionAnswer>()
                .ForMember(m => m.Content, o => o.MapFrom(s => s.Answer));

        CreateMap<QuestionAnswer, CreateAnswerDto>()
            .ForMember(m => m.Answer, o => o.MapFrom(s => s.Content));

        CreateMap<QuestionAnswer, AnswerDto>()
            .ForMember(m => m.Answer, o => o.MapFrom(s => s.Content));
        CreateMap<AnswerDto, QuestionAnswer>()
            .ForMember(m => m.Content, o => o.MapFrom(s => s.Answer));

        CreateMap<Question, QuestionDto>()
            .ForMember(m => m.Question, o => o.MapFrom(s => s.Content));
        //.ForMember(m => m.Answers, o => o.MapFrom(s => s.Answers!.Select(e => e.Content)));

        CreateMap<QuestionDto, Question>()
            .ForMember(m => m.Content, o => o.MapFrom(s => s.Question));


        // Test
        CreateMap<Test, TestDto>();
        CreateMap<TestDto, Test>();
        CreateMap<CreateTestDto, Test>();

        CreateMap<QuestionTest, QuestionDto>()
            .ForMember(m => m.Question, o => o.MapFrom(s => s.Question!.Content))
            .ForMember(m => m.Id, o => o.MapFrom(s => s.Question!.Id))
            .ForMember(m => m.QuestionType, o => o.MapFrom(s => s.Question!.QuestionType))
            .ForMember(m => m.Answers, o => o.MapFrom(s => s.Question!.Answers!.Select(e => e.Content)));

        CreateMap<Test, UserTestDto>()
            .ForMember(m => m.Author, o => o.MapFrom(s => s.User!.UserName));

        // TestSession
        CreateMap<CreateTestSessionDto, TestSession>();
        CreateMap<Test, TestSessionDto>();
        CreateMap<TestSession, UserTestSessionDto>()
            .ForMember(m => m.TestName, o => o.MapFrom(s => s.Test!.Title))
            .ForMember(m => m.PercentageScore, o => o.MapFrom(s => s.MaxScore != 0 ? (((double)s.Score / s.MaxScore) * 100.0) : -1));


        // Flashcards
        CreateMap<Flashcard, UserFlashcardDto>();
        CreateMap<FlashcardItem, FlashcardItemDto>();
        CreateMap<Flashcard, FlashcardsSetDto>();
        CreateMap<FlashcardsSetDto, Flashcard>();
        CreateMap<FlashcardItemDto, FlashcardItem>();
        
    }
}