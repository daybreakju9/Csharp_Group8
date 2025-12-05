namespace Backend.DTOs;

/// <summary>
/// 图片 DTO
/// </summary>
public class ImageDto
{
    public int Id { get; set; }
    public int QueueId { get; set; }
    public int ImageGroupId { get; set; }
    public string FolderName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public long FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? FileHash { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 图片组 DTO（包含图片列表）
/// </summary>
public class ImageGroupDto
{
    public int Id { get; set; }
    public int QueueId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int ImageCount { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ImageDto> Images { get; set; } = new List<ImageDto>();
}

/// <summary>
/// 图片组简要信息 DTO（不包含图片列表）
/// </summary>
public class ImageGroupSummaryDto
{
    public int Id { get; set; }
    public int QueueId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int ImageCount { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}


