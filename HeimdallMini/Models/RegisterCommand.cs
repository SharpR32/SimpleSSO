namespace HeimdallMini.Models
{
    public class RegisterCommand: LoginCommand
    {
        public string RepeatPassword { get; init; } = string.Empty;
    }
}
