using Microsoft.EntityFrameworkCore;
using RedisCacheDemo.Data;
using RedisCacheDemo.Services.CacheServices;
using RedisCacheDemo.Services.Department;
using RedisCacheDemo.Services.Employee;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "Redis cache";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

try
{
   var scope = app.Services.CreateAsyncScope();
   var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   await dbContext.Database.MigrateAsync();
}
catch (Exception e)
{
    app.Logger.LogError(e,"application starting error :{Message}",e.Message);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()||app.Environment.IsEnvironment("Prod"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();
