namespace QuizPlatform.Infrastructure.Entities;

public class QuestionType
{
    public int Id { get; set; }
    public QuestionTypeName Name { get; set; }
}

public enum QuestionTypeName
{
    SingleChoice,
    MultipleChoice,
    TrueFalse,
    ShortAnswer
}