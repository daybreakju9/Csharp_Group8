using System.Text.Json.Serialization;

namespace ImageAnnotationApp.Models
{
    /// 图片模型
    public class Image
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

    /// 图片组模型
    public class ImageGroup
    {
        public int Id { get; set; }
        public int QueueId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int ImageCount { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Image> Images { get; set; } = new();
    }

    /// 分页结果模型
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
