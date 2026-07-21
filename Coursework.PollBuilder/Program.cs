using Coursework.PollBuilder.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. ĐĂNG KÝ DBCONTEXT KẾT NỐI SQL SERVER
builder.Services.AddDbContext<PollBuilderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CẤU HÌNH CORS (Bắt buộc phải có để Frontend React/Vue gọi được API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Đăng ký dịch vụ SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 3. KÍCH HOẠT CORS TRONG PIPELINE (Phải đặt trước UseAuthorization)
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// KÍCH HOẠT ĐỌC FILE TĨNH (CSS, JS, Hình ảnh trong thư mục wwwroot)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers(); // Giữ lại dòng này để 4 API cũ của bạn vẫn sống khỏe
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mở endpoint cho SignalR
app.MapHub<Coursework.PollBuilder.Hubs.PollHub>("/pollHub");

// Tự động chạy Migration để tạo bảng trên Database thật
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Coursework.PollBuilder.Data.PollBuilderDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi khi tạo DB: " + ex.Message);
    }
}

app.Run();