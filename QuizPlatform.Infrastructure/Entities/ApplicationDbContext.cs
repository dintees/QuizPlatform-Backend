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
    public DbSet<UserToken> UserTokens { get; set; }
    //public DbSet<QuestionSet> QuestionSets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        //modelBuilder.Entity<Test>().HasMany(d => d.Questions).WithMany(s => s.Tests).UsingEntity<QuestionSet>();
        //modelBuilder.Entity<QuestionSet>().HasKey(i => new { i.QuestionId, i.SetId }); // to delete
    }
}