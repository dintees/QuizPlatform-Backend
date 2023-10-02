using AutoMapper;
using QuizPlatform.Infrastructure.Models.Question;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;
using QuizPlatform.Infrastructure.Models.Test;
using QuizPlatform.Infrastructure.Models.TestSession;

namespace QuizPlatform.Infrastructure.Profiles;

public class MainProfile : Profile
{
    public MainProfile()
    {
        // User
        CreateMap<UserRegisterDto, User>();
        CreateMap<User, UserDto>();
        CreateMap<ForgotPasswordDto, ChangeUserPasswordDto>();

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
        CreateMap<TestSession, UserTestSessionDto>()
            .ForMember(m => m.TestName, o => o.MapFrom(s => s.Test!.Title));
    }
}