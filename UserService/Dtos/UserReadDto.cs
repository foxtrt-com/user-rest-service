namespace UserService.Dtos;

public class UserReadDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
