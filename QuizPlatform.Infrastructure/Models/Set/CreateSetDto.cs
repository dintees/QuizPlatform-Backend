﻿using QuizPlatform.Infrastructure.Models.Question;

namespace QuizPlatform.Infrastructure.Models.Set;

public class CreateSetDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<CreateQuestionDto>? Questions { get; set; }
}