using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.API.Validation;
using QuizPlatform.Infrastructure.Entities;
using QuizPlatform.Infrastructure.Models.User;

namespace QuizPlatform.API.Extensions
{
    internal static class ServicesExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Fluent Validation
            services.AddScoped<IValidator<UserRegisterDto>, UserRegisterValidator>();
            services.AddScoped<IValidator<ChangeUserPasswordDto>, ChangeUserPasswordValidator>();
            services.AddScoped<IValidator<Question>, QuestionValidator>();
            services.AddScoped<IValidator<Set>, SetValidator>();

            // SqlServer connection
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Connection"));
            });


            services.AddCors(opts =>
            {
                opts.AddPolicy("DefaultPolicy",
                    policyOptions => { policyOptions.WithOrigins(configuration.GetSection("FrontendUrl").Value!).AllowAnyHeader().AllowAnyMethod(); });
            });
        }
    }
}
