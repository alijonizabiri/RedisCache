using System.ComponentModel.DataAnnotations;

namespace RedisCacheDemo.DTOs.Department;

public class AddDepartmentDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    [MaxLength(200)]
    public string? Description { get; set; }
}
