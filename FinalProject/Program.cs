using FinalProject.Data;
using FinalProject.Helpers;
using FinalProject.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddControllers(); 


builder.Services.AddDbContext<QlptContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PhongTro"));
});


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Customer/SignIn";
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddScoped<UserService>();

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
