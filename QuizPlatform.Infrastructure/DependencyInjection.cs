using EducationPlatform.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using QuizPlatform.Infrastructure.Entities;
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
            services.AddScoped<ISetRepository, SetRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<ISetService, SetService>();

            services.AddScoped<IUserContextService, UserContextService>();

            // AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}