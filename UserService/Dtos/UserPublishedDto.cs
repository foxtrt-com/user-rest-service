namespace UserService.Dtos;

public class UserPublishedDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
}
