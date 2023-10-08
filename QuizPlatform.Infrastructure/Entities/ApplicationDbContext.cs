using Microsoft.EntityFrameworkCore;

namespace QuizPlatform.Infrastructure.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; } 
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionAnswer> Answers { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<TestSession> TestSessions { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<UserAnswers> UserAnswers { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<FlashcardItem> FlashcardItems { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}