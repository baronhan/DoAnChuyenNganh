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

// Thêm dịch vụ session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian tồn tại của session
    options.Cookie.HttpOnly = true; // Chỉ có thể truy cập cookie từ server
    options.Cookie.IsEssential = true; // Bắt buộc nếu sử dụng cookie consent
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

// Cấu hình middleware cho session
app.UseSession(); // Thêm dòng này để kích hoạt session

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
