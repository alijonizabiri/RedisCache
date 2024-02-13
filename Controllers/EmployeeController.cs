using Microsoft.AspNetCore.Mvc;
using RedisCacheDemo.DTOs.Employee;
using RedisCacheDemo.Filters;
using RedisCacheDemo.Responses;
using RedisCacheDemo.Services.Employee;

namespace RedisCacheDemo.Controllers;
[ApiController]
[Route("api/[controller]")]
public class EmployeeController(IEmployeeService service) : ControllerBase
{
    [HttpGet]
    public async Task<Response<List<EmployeeDto>>> Employees([FromQuery] EmployeeFilter filter)
    {
        return await service.GetEmployees(filter);
    }
    
    [HttpPost]
    public async Task<Response<EmployeeDto>> AddEmployee(AddEmployeeDto employeeDto)
    {
        return await service.AddEmployee(employeeDto);
    }
    
    [HttpPut]
    public async Task<Response<EmployeeDto>> UpdateEmployee(EmployeeDto employeeDto)
    {
        return await service.UpdateEmployee(employeeDto);
    }
    
    [HttpDelete]
    public async Task<Response<string>> DeleteEmployee(int id)
    {
        return await service.DeleteEmployee(id);
    }
}
