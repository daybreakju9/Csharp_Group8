using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateSelectionDto
{
    [Required(ErrorMessage = "队列ID是必需的")]
    public int QueueId { get; set; }

    [Required(ErrorMessage = "图片组ID是必需的")]
    public int ImageGroupId { get; set; }

    [Required(ErrorMessage = "选中的图片ID是必需的")]
    public int SelectedImageId { get; set; }

    public int? DurationSeconds { get; set; }
}

public class SelectionDto
{
    public int Id { get; set; }
    public int QueueId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int ImageGroupId { get; set; }
    public string ImageGroupName { get; set; } = string.Empty;
    public int SelectedImageId { get; set; }
    public string SelectedImagePath { get; set; } = string.Empty;
    public string SelectedFolderName { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserProgressDto
{
    public int QueueId { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int CompletedGroups { get; set; }
    public int TotalGroups { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}


