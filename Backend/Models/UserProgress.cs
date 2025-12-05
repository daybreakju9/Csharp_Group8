using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// 用户进度 - 追踪用户在队列中的标注进度
/// </summary>
public class UserProgress
{
    public int Id { get; set; }

    public int QueueId { get; set; }
    public Queue Queue { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// 已完成的图片组数量
    /// </summary>
    public int CompletedGroups { get; set; } = 0;

    /// <summary>
    /// 总图片组数量
    /// </summary>
    public int TotalGroups { get; set; } = 0;

    /// <summary>
    /// 完成百分比
    /// </summary>
    public decimal ProgressPercentage => TotalGroups > 0 ? (decimal)CompletedGroups / TotalGroups * 100 : 0;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

