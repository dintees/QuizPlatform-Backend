﻿using QuizPlatform.Infrastructure.Enums;

namespace QuizPlatform.Infrastructure.Models.Question;

public class QuestionDto
{
    public int Id { get; set; }
    public string? Question { get; set; }
    public bool MathMode { get; set; }
    public bool IsDeleted { get; set; }
    public QuestionType QuestionType { get; set; }
    public List<AnswerDto>? Answers { get; set; }
}