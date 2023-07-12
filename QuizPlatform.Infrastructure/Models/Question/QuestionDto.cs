﻿using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Infrastructure.Models.Question;

public class QuestionDto
{
    public int Id { get; set; }
    public string? Question { get; set; }
    public QuestionTypeName QuestionType { get; set; }
    public List<string>? Answers { get; set; }
}