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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<TestSession>().HasOne(e => e.User).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<UserAnswers>().HasOne(e => e.TestSession).WithMany().OnDelete(DeleteBehavior.Restrict);

        //modelBuilder.Entity<Test>().HasMany(d => d.Questions).WithMany(s => s.Tests).UsingEntity<QuestionSet>();
        //modelBuilder.Entity<QuestionSet>().HasKey(i => new { i.QuestionId, i.TestId }); // to delete
    }
}