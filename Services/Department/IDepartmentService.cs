using RedisCacheDemo.DTOs.Department;
using RedisCacheDemo.Filters;
using RedisCacheDemo.Responses;

namespace RedisCacheDemo.Services.Department;

public interface IDepartmentService
{
    Task<Response<List<DepartmentDto>>> GetDepartments(DepartmentFilter filter, CancellationToken cancellationToken = default);
    Task<Response<DepartmentDto>> AddDepartment(AddDepartmentDto departmentDto, CancellationToken cancellationToken = default);
    Task<Response<DepartmentDto>> UpdateDepartment(DepartmentDto departmentDto, CancellationToken cancellationToken = default);
    Task<Response<string>> DeleteDepartment(int id, CancellationToken cancellationToken = default);
}
