using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// 图片组 - 表示一组用于对比的图片
/// </summary>
public class ImageGroup
{
    public int Id { get; set; }

    public int QueueId { get; set; }
    public Queue Queue { get; set; } = null!;

    /// <summary>
    /// 组名称 - 通常使用文件名作为组标识
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 组的显示顺序
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// 该组的图片数量（应等于 Queue.ComparisonCount）
    /// </summary>
    public int ImageCount { get; set; } = 0;

    /// <summary>
    /// 是否已完成（所有用户都已标注）
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<SelectionRecord> SelectionRecords { get; set; } = new List<SelectionRecord>();
}
