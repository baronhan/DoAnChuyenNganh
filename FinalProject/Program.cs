using FinalProject.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thêm dịch vụ API controllers
builder.Services.AddControllers(); // Thêm phần này để hỗ trợ API controllers

// Cấu hình DbContext với kết nối SQL Server
builder.Services.AddDbContext<QlptContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PhongTro"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("MyAllowSpecificOrigins");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
