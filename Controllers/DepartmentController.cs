using Microsoft.AspNetCore.Mvc;
using RedisCacheDemo.DTOs.Department;
using RedisCacheDemo.Filters;
using RedisCacheDemo.Responses;
using RedisCacheDemo.Services.Department;

namespace RedisCacheDemo.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DepartmentController(IDepartmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<Response<List<DepartmentDto>>> Departments([FromQuery] DepartmentFilter filter)
    {
        return await service.GetDepartments(filter);
    }
    
    [HttpPost]
    public async Task<Response<DepartmentDto>> AddDepartment(AddDepartmentDto departmentDto)
    {
        return await service.AddDepartment(departmentDto);
    }
    
    [HttpPut]
    public async Task<Response<DepartmentDto>> UpdateDepartment(DepartmentDto departmentDto)
    {
        return await service.UpdateDepartment(departmentDto);
    }
    
    [HttpDelete]
    public async Task<Response<string>> DeleteDepartment(int id)
    {
        return await service.DeleteDepartment(id);
    }
}
