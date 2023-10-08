using EducationPlatform.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using QuizPlatform.Infrastructure.Builders;
using QuizPlatform.Infrastructure.Interfaces;
using QuizPlatform.Infrastructure.Repositories;
using QuizPlatform.Infrastructure.Services;

namespace QuizPlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();
            services.AddScoped<ITestSessionRepository, TestSessionRepository>();
            services.AddScoped<IUserAnswersRepository, UserAnswersRepository>();
            services.AddScoped<IFlashcardRepository, FlashcardRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<ITestSessionService, TestSessionService>();
            services.AddScoped<IFlashcardService, FlashcardService>();

            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<EmailBuilder>();

            // AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}