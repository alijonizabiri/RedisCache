using System.Net;
using Microsoft.EntityFrameworkCore;
using RedisCacheDemo.Data;
using RedisCacheDemo.DefaultCacheKeys;
using RedisCacheDemo.DTOs.Employee;
using RedisCacheDemo.Filters;
using RedisCacheDemo.Responses;
using RedisCacheDemo.Services.CacheServices;

namespace RedisCacheDemo.Services.Employee;

public class EmployeeService(
    ApplicationDbContext context, 
    ILogger<EmployeeService> logger, 
    ICacheService cacheService)
    : IEmployeeService
{
    public async Task<Response<List<EmployeeDto>>> GetEmployees(EmployeeFilter filter, CancellationToken cancellationToken = default)
    {
        var employeesInCache = await cacheService.GetAsync<List<EmployeeDto>>(DefaultKey.Employee, cancellationToken);
        if (employeesInCache != null)
        {
            if (filter.Name != null)
            {
                employeesInCache = employeesInCache.Where(x => 
                        string.Concat(x.FirstName, x.LastName).ToLower().Trim()
                        .Contains(filter.Name.ToLower().Trim())).ToList();
            }

            if (filter.PhoneNumber != null)
            {
                employeesInCache = employeesInCache.Where(x => x.PhoneNumber == filter.PhoneNumber).ToList();
            }
            return new Response<List<EmployeeDto>>(employeesInCache);
        }
        
        await Console.Out.WriteLineAsync(new string('*', 120));
        logger.LogInformation("Retrieving data from database");
        await Console.Out.WriteLineAsync(new string('*', 120));
        
        var employees = context.Employees.AsQueryable();
        
        if (filter.Name != null)
        {
            employees = employees.Where(x => 
                string.Concat(x.FirstName, x.LastName).ToLower().Trim()
                    .Contains(filter.Name.ToLower().Trim()));
        }

        if (filter.PhoneNumber != null)
        {
            employees = employees.Where(x => x.PhoneNumber == filter.PhoneNumber);
        }

        var data = await employees.Select(s=> new EmployeeDto()
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            PhoneNumber = s.PhoneNumber,
            Address = s.Address,
            Email = s.Email,
            Gender = s.Gender,
            BirthDate = s.BirthDate,
            DepartmentId = s.DepartmentId
        }).ToListAsync(cancellationToken: cancellationToken);

        return new Response<List<EmployeeDto>>(data);
    }

    public async Task<Response<EmployeeDto>> AddEmployee(AddEmployeeDto employeeDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.PhoneNumber == employeeDto.PhoneNumber,
                cancellationToken: cancellationToken);

            if (employee != null)
                return new Response<EmployeeDto>(HttpStatusCode.BadRequest,
                    "Employee with this phone number already exists");

            var newEmployee = new Entities.Employee
            {
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                PhoneNumber = employeeDto.PhoneNumber,
                Address = employeeDto.Address,
                Email = employeeDto.Email,
                Gender = employeeDto.Gender,
                BirthDate = employeeDto.BirthDate,
                DepartmentId = employeeDto.DepartmentId
            };

            await context.Employees.AddAsync(newEmployee, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            var data = new EmployeeDto()
            {
                Id = newEmployee.Id,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                PhoneNumber = employeeDto.PhoneNumber,
                Address = employeeDto.Address,
                Email = employeeDto.Email,
                Gender = employeeDto.Gender,
                BirthDate = employeeDto.BirthDate,
                DepartmentId = employeeDto.DepartmentId
            };
            _ = Task.Run(async () =>
            {
                var employees = await cacheService.GetAsync<List<EmployeeDto>>(DefaultKey.Employee, cancellationToken);
                employees?.Add(data);

                var expirationTime = DateTimeOffset.Now.AddMinutes(2);
                await cacheService.AddAsync(DefaultKey.Employee, employees, expirationTime, cancellationToken);
            }, cancellationToken);
            
            return new Response<EmployeeDto>(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new Response<EmployeeDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<EmployeeDto>> UpdateEmployee(EmployeeDto employeeDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == employeeDto.Id,
                cancellationToken: cancellationToken);
            if (employee == null)
                return new Response<EmployeeDto>(HttpStatusCode.NotFound, "Employee not found");

            var existingPhoneNumber = await context.Employees.FirstOrDefaultAsync(
                e => employee.PhoneNumber != employeeDto.PhoneNumber && e.PhoneNumber == employeeDto.PhoneNumber,
                cancellationToken: cancellationToken);
            if (existingPhoneNumber != null)
                return new Response<EmployeeDto>(HttpStatusCode.BadRequest,
                    "Employee with this phone number already exists");

            employee.FirstName = employeeDto.FirstName;
            employee.LastName = employeeDto.LastName;
            employee.PhoneNumber = employeeDto.PhoneNumber;
            employee.Address = employeeDto.Address;
            employee.Email = employeeDto.Email;
            employee.Gender = employeeDto.Gender;
            employee.BirthDate = employeeDto.BirthDate;
            employee.DepartmentId = employeeDto.DepartmentId;

            await context.SaveChangesAsync(cancellationToken);

            var data = new EmployeeDto()
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                Email = employee.Email,
                Gender = employee.Gender,
                BirthDate = employee.BirthDate,
                DepartmentId = employee.DepartmentId
            };
            _ = Task.Run(async () =>
            {
                var employeesInCache =
                    await cacheService.GetAsync<List<EmployeeDto>>(DefaultKey.Employee, cancellationToken);
                
                if (employeesInCache != null)
                {
                    var emp = employeesInCache.FirstOrDefault(e => e.Id == employee.Id);
                    if (emp != null)
                    {
                        var index = employeesInCache.FindIndex(e => e.Id == employee.Id);
                        employeesInCache[index] = data;
                    }
                    else
                    {
                        employeesInCache.Add(data);
                    }

                    var expirationTime = DateTimeOffset.UtcNow.AddMinutes(2);
                    await cacheService.AddAsync(DefaultKey.Employee, employeesInCache, expirationTime,
                        cancellationToken);
                }
            }, cancellationToken);

            return new Response<EmployeeDto>(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new Response<EmployeeDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> DeleteEmployee(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (employee == null) return new Response<string>(HttpStatusCode.NotFound, "Employee not found");

            context.Employees.Remove(employee);

            _ = Task.Run(async () =>
            {
                var employeesInCache =
                    await cacheService.GetAsync<List<EmployeeDto>>(DefaultKey.Employee, cancellationToken);
                if (employeesInCache != null)
                {
                    var emp = employeesInCache.FirstOrDefault(e => e.Id == employee.Id);
                    if (emp != null)
                        employeesInCache.Remove(emp);
                    else
                        logger.LogError("Employee not found");
                }
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return new Response<string>("Employee deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured while deleting employee: {0}", ex.Message);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}
