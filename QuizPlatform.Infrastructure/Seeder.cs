using QuizPlatform.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizPlatform.Infrastructure
{
    public class Seeder
    {
        private readonly ApplicationDbContext _context;

        public Seeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Seed()
        {
            if (await _context.Database.CanConnectAsync())
            {
                var roleTypes = getRoleTypes();
                await _context.Roles.AddRangeAsync(roleTypes);

                var questionTypes = getQuestionTypes();
                await _context.QuestionTypes.AddRangeAsync(questionTypes);


                await _context.SaveChangesAsync();
            }
        }

        private IEnumerable<Role> getRoleTypes()
        {
            return new List<Role>()
            {
                new Role { Name = RoleTypeName.Admin },
                new Role { Name = RoleTypeName.Teacher },
                new Role { Name = RoleTypeName.User }
            };
        }

        private IEnumerable<QuestionType> getQuestionTypes()
        {
            return new List<QuestionType>
            {
                new QuestionType { Name = QuestionTypeName.SingleChoice },
                new QuestionType { Name = QuestionTypeName.MultipleChoice },
                new QuestionType { Name = QuestionTypeName.TrueFalse },
                new QuestionType { Name = QuestionTypeName.ShortAnswer }
            };
        }
    }
}
