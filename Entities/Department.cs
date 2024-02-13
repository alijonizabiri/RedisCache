namespace RedisCacheDemo.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public virtual List<Employee> Employees { get; set; } = null!;
}
