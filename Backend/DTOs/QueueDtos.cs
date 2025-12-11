using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateQueueDto
{
    [Required(ErrorMessage = "项目ID是必需的")]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "队列名称是必需的")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "对比图片数量是必需的")]
    [Range(2, 10, ErrorMessage = "对比图片数量必须在2到10之间")]
    public int ComparisonCount { get; set; } = 2;

    public bool IsRandomOrder { get; set; } = false;
}

public class UpdateQueueDto
{
    [Required(ErrorMessage = "队列名称是必需的")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "对比图片数量是必需的")]
    [Range(2, 10, ErrorMessage = "对比图片数量必须在2到10之间")]
    public int ComparisonCount { get; set; }

    public string? Status { get; set; }

    public bool? IsRandomOrder { get; set; }
}

public class QueueDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ComparisonCount { get; set; }
    public int GroupCount { get; set; }
    public int TotalImageCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRandomOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

