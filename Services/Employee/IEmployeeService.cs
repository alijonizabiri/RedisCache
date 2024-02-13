using RedisCacheDemo.DTOs.Employee;
using RedisCacheDemo.Filters;
using RedisCacheDemo.Responses;

namespace RedisCacheDemo.Services.Employee;

public interface IEmployeeService
{
    Task<Response<List<EmployeeDto>>> GetEmployees(EmployeeFilter filter, CancellationToken cancellationToken = default);
    Task<Response<EmployeeDto>> AddEmployee(AddEmployeeDto employeeDto, CancellationToken cancellationToken = default);
    Task<Response<EmployeeDto>> UpdateEmployee(EmployeeDto employeeDto, CancellationToken cancellationToken = default);
    Task<Response<string>> DeleteEmployee(int id, CancellationToken cancellationToken = default);
}
