namespace HeimdallMini.Models
{
    public class ChangePasswordCommand
    {
        public string OldPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }
}
