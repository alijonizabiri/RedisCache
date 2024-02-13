using System.ComponentModel.DataAnnotations;
using RedisCacheDemo.Enums;

namespace RedisCacheDemo.DTOs.Employee;

public class AddEmployeeDto
{
    [Required]
    [MaxLength(30, ErrorMessage = "First name can not be greater than 30 characters")]
    public string FirstName { get; set; } = null!;
    [Required]
    [MaxLength(30, ErrorMessage = "Last name can not be greater than 30 characters")]
    public string LastName { get; set; } = null!;
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = null!;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
    public Gender Gender { get; set; } 
    public int DepartmentId { get; set; }
}
