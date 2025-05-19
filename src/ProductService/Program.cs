using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using PrivateUtilities.Models;
using PrivateUtilities.EventBus;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load("../../.env");

// Veritabanı bağlantısını ekliyoruz
var config = builder.Configuration;

var connectionString = config["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new ArgumentNullException("Connection string is not set in Env.");}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // 0.0.0.0:80'de dinle
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// CORS ayarlarını kullan
app.UseCors("AllowAll");


// Statik dosya servisini aç
app.UseDefaultFiles(); // index.html gibi dosyaları otomatik yükler
app.UseStaticFiles();  // wwwroot klasörüne erişim sağlar

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();



