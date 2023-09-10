namespace QuizPlatform.Infrastructure.ErrorMessages
{
    public static class UserErrorMessages
    {
        public const string EmptyEmail = "Email could not be empty.";
        public const string EmptyUserName = "User name could not be empty.";
        public const string EmptyFirstName = "First name could not be empty.";
        public const string EmptyLastName = "Last name could not be empty.";
        public const string EmptyPassword = "Password could not be empty.";
        public const string WrongEmailFormat = "Wrong email format.";
        public const string TooShortPassword = "Password must be at least 8 characters long.";
        public const string UserAlreadyExistsError = "User with this username or email already exists.";
        public const string NotTheSamePasswords = "Given passwords are not the same.";
        public const string CurrentPasswordIsIncorrect = "The current password is wrong.";
        public const string PersonWithThisIdDoesNotExist = "The person with this id does not exist";
        public const string RoleCouldNotBeEmpty = "Role could not be empty.";
        public const string AccountNotConfirmed = "Wrong activation code. The account has not been confirmed.";
    }
}
