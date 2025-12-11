using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// 队列 - 表示一个标注任务队列
/// </summary>
public class Queue
{
    public int Id { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    /// <summary>
    /// 队列名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 队列描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 每组对比的图片数量（2-10张）
    /// </summary>
    [Range(2, 10)]
    public int ComparisonCount { get; set; } = 2;

    /// <summary>
    /// 总图片组数
    /// </summary>
    public int GroupCount { get; set; } = 0;

    /// <summary>
    /// 总图片数
    /// </summary>
    public int TotalImageCount { get; set; } = 0;

    /// <summary>
    /// 队列状态
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = QueueStatus.Draft;

    /// <summary>
    /// 是否启用随机顺序展示
    /// </summary>
    public bool IsRandomOrder { get; set; } = false;

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ImageGroup> ImageGroups { get; set; } = new List<ImageGroup>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<SelectionRecord> SelectionRecords { get; set; } = new List<SelectionRecord>();
    public ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
}

/// <summary>
/// 队列状态常量
/// </summary>
public static class QueueStatus
{
    public const string Draft = "Draft";           // 草稿
    public const string Active = "Active";         // 进行中
    public const string Completed = "Completed";   // 已完成
    public const string Archived = "Archived";     // 已归档
}

