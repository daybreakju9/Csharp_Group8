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

// 减少EF Core的SQL日志噪音（只记录警告和错误）
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
// 大文件上传
builder.WebHost.ConfigureKestrel(opt =>
{
    opt.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024;
    opt.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<FormOptions>(opt =>
{
    opt.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024;
    opt.ValueLengthLimit = int.MaxValue;
    opt.MultipartHeadersLengthLimit = int.MaxValue;
});

// Controllers + 统一模型验证返回
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ModelValidation");

        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => new { Field = e.Key, Errors = e.Value!.Errors.Select(x => x.ErrorMessage) })
            .ToList();

        var correlationId = context.HttpContext.TraceIdentifier;
        logger.LogWarning("模型验证失败 Path={Path} CorrelationId={CorrelationId} Errors={Errors}",
            context.HttpContext.Request.Path, correlationId, JsonSerializer.Serialize(errors));

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

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IImageGroupService, ImageGroupService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<ISelectionService, SelectionService>();
builder.Services.AddScoped<IUserService, UserService>();


// Configure SQLite Database with optimized settings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30); // 30秒命令超时
        sqliteOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
    });
    
    // 开发环境启用详细日志
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// JWT 强密钥校验
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetBytes(secretKey).Length < 32)
    throw new InvalidOperationException("JWT SecretKey 必须 ≥32 字节（256 bit）");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// CORS 可配置
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5174", "http://localhost:3000" };

builder.Services.AddCors(opt => opt.AddPolicy("AllowFrontend", p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services.AddHealthChecks();

var app = builder.Build();

// 异步迁移 + 30秒超时保护
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("正在应用数据库迁移...");
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await db.Database.MigrateAsync(cts.Token);
        logger.LogInformation("数据库迁移完成");

        // 仅开发环境可选自动创建管理员
        if (app.Environment.IsDevelopment() &&
            builder.Configuration.GetValue<bool>("Auth:CreateDefaultAdmin", false))
        {
            if (!await db.Users.AnyAsync(u => u.Username == "admin"))
            {
                db.Users.Add(new Backend.Models.User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                logger.LogWarning("【开发环境】已自动创建默认管理员：admin / Admin@123");
            }
        }
    }
    catch (OperationCanceledException)
    {
        logger.LogError("数据库迁移超时（30秒），请检查 database.db 是否被其他程序占用");
        throw;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "数据库迁移失败");
        throw;
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseCors("AllowFrontend");
// Serve static files (uploaded images)
var uploadRoot = builder.Configuration["Storage:UploadRoot"];

// 静态文件：长缓存 + 防盗链
var uploadRoot = builder.Configuration["Storage:UploadRoot"] ?? "uploads";
if (!Path.IsPathFullyQualified(uploadRoot))
    uploadRoot = Path.Combine(app.Environment.ContentRootPath, uploadRoot);

Directory.CreateDirectory(uploadRoot);

var allowedReferers = builder.Configuration
    .GetSection("Security:AllowedReferers")
    .Get<string[]>()
    ?? new[] { "http://localhost:5174", "http://localhost:3000" };


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // 图片缓存1小时，减少服务器压力
        ctx.Context.Response.Headers.Append(
            "Cache-Control", 
            app.Environment.IsDevelopment() 
                ? "no-cache" 
                : "public, max-age=3600");
    }
});

// 🔧【修改这里】调整中间件顺序
// 全局异常处理（必须在最前面）
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 请求/响应日志（现在只记录，不再处理异常）
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();