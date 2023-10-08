using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Configurations
{
    public class UserAnswersConfiguration : IEntityTypeConfiguration<UserAnswers>
    {
        public void Configure(EntityTypeBuilder<UserAnswers> builder)
        {
            builder.HasOne(e => e.TestSession).WithMany().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
