using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// 图片 - 存储单个图片文件的信息
/// </summary>
public class Image
{
    public int Id { get; set; }

    public int QueueId { get; set; }
    public Queue Queue { get; set; } = null!;

    /// <summary>
    /// 所属图片组ID
    /// </summary>
    public int ImageGroupId { get; set; }
    public ImageGroup ImageGroup { get; set; } = null!;

    /// <summary>
    /// 文件夹名称（来源文件夹）
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// 原始文件名
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件存储路径
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 在组内的显示顺序
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; } = 0;

    /// <summary>
    /// 图片宽度（像素）
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// 图片高度（像素）
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// 文件MD5哈希值（用于去重和完整性校验）
    /// </summary>
    [MaxLength(32)]
    public string? FileHash { get; set; }

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<SelectionRecord> SelectionRecords { get; set; } = new List<SelectionRecord>();
}

