namespace TaskTracer.UserService.Models
{
    public class PasswordUpdateRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}