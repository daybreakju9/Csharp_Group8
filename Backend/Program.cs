using Backend.Data;
using Backend.Middleware;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure Kestrel server limits for file uploads
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024; // 2 GB
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// Configure form options for multipart/form-data
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; // 2 GB
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ModelValidation");

        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value!.Errors.Select(err => err.ErrorMessage)
            })
            .ToList();

        var correlationId = context.HttpContext.TraceIdentifier;

        logger.LogWarning("模型验证失败 Path={Path} CorrelationId={CorrelationId} Errors={Errors}",
            context.HttpContext.Request.Path,
            correlationId,
            JsonSerializer.Serialize(errors));

        return new BadRequestObjectResult(new
        {
            message = "请求数据验证失败",
            correlationId,
            errors
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Repository layer
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Service layer
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IImageGroupService, ImageGroupService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<ISelectionService, SelectionService>();
builder.Services.AddScoped<IUserService, UserService>();

// Configure SQLite Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Apply migrations
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");

        // 创建默认管理员账号
        if (!dbContext.Users.Any(u => u.Username == "admin"))
        {
            var adminUser = new Backend.Models.User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
            Console.WriteLine("Default admin user created: admin/Admin@123");
        }
        else
        {
            Console.WriteLine("Admin user already exists.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Serve static files (uploaded images)
/*app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});*/

var uploadRoot = builder.Configuration["Storage:UploadRoot"];

if (!Path.IsPathFullyQualified(uploadRoot))
{
    uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), uploadRoot);
}

// Ensure directory exists (auto-create)
if (!Directory.Exists(uploadRoot))
{
    Directory.CreateDirectory(uploadRoot);
    Console.WriteLine($"[Storage] Upload directory created at: {uploadRoot}");
}
else
{
    Console.WriteLine($"[Storage] Upload directory already exists: {uploadRoot}");
}

// Serve static files from this directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads"
});

// 请求/响应日志（含 4xx/5xx 与异常）
app.UseMiddleware<RequestResponseLoggingMiddleware>();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
