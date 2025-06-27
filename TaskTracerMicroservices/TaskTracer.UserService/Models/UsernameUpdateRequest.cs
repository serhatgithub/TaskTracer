namespace TaskTracer.UserService.Models
{
    // Bu DTO'yu doğrudan controller içinde de tanımlayabilirdik
    // ancak ayrı bir dosyada olması daha düzenli.
    // Orijinal projede bu `UsersController` içinde tanımlıydı.
    public record UsernameUpdateRequest(string NewUsername);
}