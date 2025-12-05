using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// 选择记录 - 记录用户对图片组的选择
/// </summary>
public class SelectionRecord
{
    public int Id { get; set; }

    public int QueueId { get; set; }
    public Queue Queue { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// 选择的图片组ID
    /// </summary>
    public int ImageGroupId { get; set; }
    public ImageGroup ImageGroup { get; set; } = null!;

    /// <summary>
    /// 用户选择的图片ID
    /// </summary>
    public int SelectedImageId { get; set; }
    public Image SelectedImage { get; set; } = null!;

    /// <summary>
    /// 选择耗时（秒）
    /// </summary>
    public int? DurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

