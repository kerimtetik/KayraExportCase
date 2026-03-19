namespace Auth.Application.DTOs;

public class AssignRoleRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
