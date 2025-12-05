using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// 项目 - 最顶层的组织单位
/// </summary>
public class Project
{
    public int Id { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 项目描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Queue> Queues { get; set; } = new List<Queue>();
}

