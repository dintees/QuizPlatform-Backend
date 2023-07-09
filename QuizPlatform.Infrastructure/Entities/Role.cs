namespace QuizPlatform.Infrastructure.Entities;

public class Role
{
    public int Id { get; set; }
    public RoleTypeName Name { get; set; }
}

public enum RoleTypeName
{
    Admin,
    Teacher,
    User
}