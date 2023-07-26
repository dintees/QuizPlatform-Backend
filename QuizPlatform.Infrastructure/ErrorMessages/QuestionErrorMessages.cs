namespace QuizPlatform.Infrastructure.ErrorMessages
{
    public static class QuestionErrorMessages
    {
        public const string QuestionDoesNotExist = "Question with the given id does not exist.";
        public const string EmptyQuestionContent = "Question could not be empty.";
        public const string EmptyAnswersContent = "Answers could not be empty.";
        public const string WrongNumberOfCorrectAnswers = "Wrong number of correct answers.";
        public const string WrongNumberOfAnswers = "Wrong number of answers.";
        public const string OneAnswerShouldBeCorrect = "One answer must be correct in this type of question.";
        public const string InvalidQuestionType = "Invalid question type";
    }
}
