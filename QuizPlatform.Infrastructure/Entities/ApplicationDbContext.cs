using System.Collections.Immutable;
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
    public DbSet<Set> Sets { get; set; }
    //public DbSet<QuestionSet> QuestionSets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>().HasData(new[]
        {
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" },
        });
        //modelBuilder.Entity<Set>().HasMany(d => d.Questions).WithMany(s => s.Sets).UsingEntity<QuestionSet>();
        //modelBuilder.Entity<QuestionSet>().HasKey(i => new { i.QuestionId, i.SetId }); // to del
    }
}