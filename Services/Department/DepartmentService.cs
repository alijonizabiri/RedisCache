using System.Net;
using Microsoft.EntityFrameworkCore;
using RedisCacheDemo.Data;
using RedisCacheDemo.DefaultCacheKeys;
using RedisCacheDemo.DTOs.Department;
using RedisCacheDemo.Filters;
using RedisCacheDemo.Responses;
using RedisCacheDemo.Services.CacheServices;

namespace RedisCacheDemo.Services.Department;

public class DepartmentService(
    ApplicationDbContext context, 
    ILogger<DepartmentService> logger, 
    ICacheService cacheService)
    : IDepartmentService
{
    public async Task<Response<List<DepartmentDto>>> GetDepartments(DepartmentFilter filter, CancellationToken cancellationToken = default)
    {
        var departmentsInCache = await cacheService.GetAsync<List<DepartmentDto>>(DefaultKey.Department, cancellationToken);
        if (departmentsInCache != null)
        {
            if (filter.Name != null)
            {
                departmentsInCache = departmentsInCache.Where(x => x.Name.ToLower().Trim()
                        .Contains(filter.Name.ToLower().Trim())).ToList();
            }
            
            return new Response<List<DepartmentDto>>(departmentsInCache);
        }
        
        await Console.Out.WriteLineAsync(new string('*', 120));
        logger.LogInformation("Retrieving data from database");
        await Console.Out.WriteLineAsync(new string('*', 120));
        
        var departments = context.Departments.AsQueryable();
        
        if (filter.Name != null)
        {
            departments = departments.Where(x => x.Name.ToLower().Trim()
                .Contains(filter.Name.ToLower().Trim()));
        }

        var data = await departments.Select(d=> new DepartmentDto()
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description
        }).ToListAsync(cancellationToken: cancellationToken);

        if (data.Count == 0 || filter.Name != null) return new Response<List<DepartmentDto>>(data);
        
        var expirationTime = DateTimeOffset.Now.AddMinutes(2);
        await cacheService.AddAsync(DefaultKey.Department, data, expirationTime, cancellationToken);

        return new Response<List<DepartmentDto>>(data);
    }

    public async Task<Response<DepartmentDto>> AddDepartment(AddDepartmentDto departmentDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var department = new Entities.Department
            {
                Name = departmentDto.Name,
                Description = departmentDto.Description
            };

            await context.Departments.AddAsync(department, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            var data = new DepartmentDto()
            {
                Id = department.Id,
                Name = departmentDto.Name,
                Description = departmentDto.Description
            };
            _ = Task.Run(async () =>
            {
                var departments = await cacheService.GetAsync<List<DepartmentDto>>(DefaultKey.Department, cancellationToken);
                if (departments != null)
                {
                    departments.Add(data);
                    var expirationTime = DateTimeOffset.Now.AddMinutes(2);
                    await cacheService.AddAsync(DefaultKey.Department, departments, expirationTime, cancellationToken);
                }
            }, cancellationToken);
            
            return new Response<DepartmentDto>(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new Response<DepartmentDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<DepartmentDto>> UpdateDepartment(DepartmentDto departmentDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await context.Departments.FirstOrDefaultAsync(e => e.Id == departmentDto.Id,
                cancellationToken: cancellationToken);
            if (department == null)
                return new Response<DepartmentDto>(HttpStatusCode.NotFound, "Department not found");
            
            department.Name = departmentDto.Name;
            department.Description = departmentDto.Description;
            
            await context.SaveChangesAsync(cancellationToken);

            var data = new DepartmentDto()
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };
            _ = Task.Run(async () =>
            {
                var departmentsInCache =
                    await cacheService.GetAsync<List<DepartmentDto>>(DefaultKey.Department, cancellationToken);
                
                if (departmentsInCache != null)
                {
                    var emp = departmentsInCache.FirstOrDefault(e => e.Id == department.Id);
                    if (emp != null)
                    {
                        var index = departmentsInCache.FindIndex(e => e.Id == department.Id);
                        departmentsInCache[index] = data;
                    }
                    else
                    {
                        departmentsInCache.Add(data);
                    }

                    var expirationTime = DateTimeOffset.UtcNow.AddMinutes(2);
                    await cacheService.AddAsync(DefaultKey.Department, departmentsInCache, expirationTime,
                        cancellationToken);
                }
            }, cancellationToken);

            return new Response<DepartmentDto>(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return new Response<DepartmentDto>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    public async Task<Response<string>> DeleteDepartment(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await context.Departments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (department == null) return new Response<string>(HttpStatusCode.NotFound, "Department not found");

            context.Departments.Remove(department);

            _ = Task.Run(async () =>
            {
                var departmentsInCache =
                    await cacheService.GetAsync<List<DepartmentDto>>(DefaultKey.Department, cancellationToken);
                if (departmentsInCache != null)
                {
                    var emp = departmentsInCache.FirstOrDefault(e => e.Id == department.Id);
                    if (emp != null)
                    {
                        departmentsInCache.Remove(emp);
                        await cacheService.AddAsync(DefaultKey.Department, departmentsInCache,
                            DateTimeOffset.Now.AddMinutes(2), cancellationToken);
                    }
                    else
                        logger.LogError("Department not found");
                }
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return new Response<string>("Department deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError("Error occured while deleting Department: {0}", ex.Message);
            return new Response<string>(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}

