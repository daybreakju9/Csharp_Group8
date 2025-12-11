using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Backend.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // 设计时使用的连接字符串（用于生成迁移）
        var connectionString = "Server=localhost;Port=3306;Database=image_selection_design_db;Uid=root;Pwd=password;";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 35)); // 根据你的MySQL版本调整

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AppDbContext(optionsBuilder.Options);
    }
}

