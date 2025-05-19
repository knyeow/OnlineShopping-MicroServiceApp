using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrivateUtilities.EventBus;
using PrivateUtilities.JWT;
var builder = WebApplication.CreateBuilder(args);



// Veritabanı bağlantısını ekliyoruz
DotNetEnv.Env.Load("../../.env");

// Veritabanı bağlantısını ekliyoruz

var config = builder.Configuration;
var connectionString = config["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new ArgumentNullException("Connection string is not set in Env.");
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // 0.0.0.0:80'de dinle
});


builder.Services.AddHttpClient<ProductClient>(client =>
{
    client.BaseAddress = new Uri("http://productservice"); // docker-compose içindeki service name
});

builder.Services.AddHttpClient<UserClient>(client =>
{
    client.BaseAddress = new Uri("http://userservice"); // docker-compose içindeki service name
});



var configKey = config["Jwt:Key"];

if (string.IsNullOrEmpty(configKey))
{
    throw new ArgumentNullException("JWT key is not set in configuration.");
}

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey))
        };
    });

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthorization();

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

// CORS ayarlarını uyguluyoruz
app.UseCors("AllowAll");

// Statik dosya servisini aç
app.UseDefaultFiles(); // index.html gibi dosyaları otomatik yükler
app.UseStaticFiles();  // wwwroot klasörüne erişim sağlar

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();



