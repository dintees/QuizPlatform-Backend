using AutoMapper;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.Infrastructure.Profiles;

public class MainProfile : Profile
{
    public MainProfile()
    {
        // User
        CreateMap<UserRegisterDto, User>();

        // Question
        CreateMap<CreateQuestionDto, Question>()
            .ForMember(m => m.Content, c => c.MapFrom(s => s.Question));

        CreateMap<CreateAnswerDto, QuestionAnswer>()
                .ForMember(m => m.Content, o => o.MapFrom(s => s.Answer));

        CreateMap<Question, QuestionDto>()
            .ForMember(m => m.Question, o => o.MapFrom(s => s.Content))
            .ForMember(m => m.Answers, o => o.MapFrom(s => s.Answers!.Select(e => e.Content)));

        // Set
        CreateMap<Set, SetDto>();
        CreateMap<SetDto, Set>();
        CreateMap<CreateSetDto, Set>();

        CreateMap<QuestionSet, QuestionDto>()
            .ForMember(m => m.Question, o => o.MapFrom(s => s.Question!.Content))
            .ForMember(m => m.Id, o => o.MapFrom(s => s.Question!.Id))
            .ForMember(m => m.QuestionType, o => o.MapFrom(s => s.Question!.QuestionType))
            .ForMember(m => m.Answers, o => o.MapFrom(s => s.Question!.Answers!.Select(e => e.Content)));

        CreateMap<Set, UserSetDto>();
    }
}