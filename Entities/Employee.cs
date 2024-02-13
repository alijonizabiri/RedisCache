using RedisCacheDemo.Enums;

namespace RedisCacheDemo.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
    public Gender Gender { get; set; } 
    public int DepartmentId { get; set; }
    public virtual Department Department { get; set; } = null!;
}
