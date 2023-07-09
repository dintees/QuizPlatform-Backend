using AutoMapper;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;
using QuizPlatform.Infrastructure.Models.Set;

namespace QuizPlatform.Infrastructure.Profiles;

public class AutoMapper : Profile
{
    public AutoMapper()
    {
        CreateMap<UserRegisterDto, User>();

        CreateMap<CreateQuestionDto, Question>()
            .ForMember(m => m.Content, c => c.MapFrom(s => s.Question))
            .ForMember(m => m.IdType, o => o.MapFrom(s => s.QuestionType));
        CreateMap<Question, CreateQuestionDto>()
            .ForMember(m => m.QuestionType, o => o.MapFrom(s => s.IdType))
            .ForMember(m => m.Question, o => o.MapFrom(s => s.Content));

        CreateMap<CreateAnswerDto, QuestionAnswer>()
                .ForMember(m => m.Content, o => o.MapFrom(s => s.Answer));
        CreateMap<QuestionAnswer, CreateAnswerDto>()
            .ForMember(m => m.Answer, o => o.MapFrom(s => s.Content));

        CreateMap<Set, SetDto>()
            .ForMember(m => m.SetId, o => o.MapFrom(s => s.Id));
        CreateMap<QuestionSet, QuestionDto>()
            .ForMember(m => m.Question, o => o.MapFrom(s => s.Question!.Content))
            .ForMember(m => m.QuestionType, o => o.MapFrom(s => s.Question!.IdType))
            .ForMember(m => m.Answers, o => o.MapFrom(s => s.Question!.Answers!.Select(e => e.Content)));
    }
}