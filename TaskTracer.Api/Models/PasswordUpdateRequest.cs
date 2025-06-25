namespace TaskTracer.Api.Models;

public class PasswordUpdateRequest
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
