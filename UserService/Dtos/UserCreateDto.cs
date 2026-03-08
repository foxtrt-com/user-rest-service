using System.ComponentModel.DataAnnotations;

namespace UserService.Dtos;

public class UserCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Email { get; set; } = string.Empty;
}
