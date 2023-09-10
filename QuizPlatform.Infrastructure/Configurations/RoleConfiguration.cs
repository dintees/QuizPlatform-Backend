using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData(new[]
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" },
            });
        }
    }
}
