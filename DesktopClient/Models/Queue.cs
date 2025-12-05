namespace ImageAnnotationApp.Models
{
    public class Queue
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

    public class CreateQueueDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ComparisonCount { get; set; } = 2;
        public bool IsRandomOrder { get; set; } = false;
    }

    public class UpdateQueueDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ComparisonCount { get; set; }
        public string? Status { get; set; }
        public bool? IsRandomOrder { get; set; }
    }
}
