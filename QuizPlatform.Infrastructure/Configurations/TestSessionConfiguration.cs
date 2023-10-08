using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Configurations
{
    public class TestSessionConfiguration : IEntityTypeConfiguration<TestSession>
    {
        public void Configure(EntityTypeBuilder<TestSession> builder)
        {
            builder.HasOne(e => e.User).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
