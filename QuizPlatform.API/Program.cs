using System.Text;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using QuizPlatform.API.Extensions;
using QuizPlatform.API.Middlewares;
using QuizPlatform.Infrastructure;
using QuizPlatform.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

// AddAsync services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure DI
builder.Services.AddInfrastructure();

// Configure services
builder.Services.ConfigureServices(builder.Configuration);

// Middlewares
builder.Services.AddScoped<ErrorHandlingMiddleware>();


// JWT settings
var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("JWT").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);

// Email configuration
var emailConfiguration = new EmailConfiguration();
builder.Configuration.GetSection("EmailConfiguration").Bind(emailConfiguration);
builder.Services.AddSingleton(emailConfiguration);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = "Bearer";
    option.DefaultScheme = "Bearer";
    option.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = authenticationSettings.Issuer,
        ValidAudience = authenticationSettings.Issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.Key!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

//app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();