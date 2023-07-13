using Microsoft.Extensions.DependencyInjection;
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

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<ISetService, SetService>();

            // AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Seeder
            //services.AddScoped<Seeder>();
        }
    }
}