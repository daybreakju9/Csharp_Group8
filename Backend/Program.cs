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

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));

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
        // 30 天强缓存
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=2592000, immutable";

        // 防盗链
        var referer = ctx.Context.Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer) &&
            !allowedReferers.Any(r => referer.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
        {
            ctx.Context.Response.StatusCode = 403;
            ctx.Context.Response.ContentType = "text/plain";
            ctx.Context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Forbidden: invalid referer")).AsTask().Wait();
            return; // 直接返回，阻止文件发送
        }
    }
});

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();