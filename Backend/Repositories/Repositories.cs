using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<Project?> GetWithQueuesAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Queues)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetByCreatorIdAsync(int creatorId)
    {
        return await _context.Projects
            .Where(p => p.CreatedById == creatorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}

public class QueueRepository : Repository<Queue>, IQueueRepository
{
    public QueueRepository(AppDbContext context) : base(context) { }

    public async Task<Queue?> GetWithImagesAsync(int id)
    {
        return await _context.Queues
            .Include(q => q.ImageGroups)
            .ThenInclude(g => g.Images)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Queue>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Queues
            .Where(q => q.ProjectId == projectId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Queue>> GetByStatusAsync(string status)
    {
        return await _context.Queues
            .Where(q => q.Status == status)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }
}

public class ImageGroupRepository : Repository<ImageGroup>, IImageGroupRepository
{
    public ImageGroupRepository(AppDbContext context) : base(context) { }

    public async Task<ImageGroup?> GetWithImagesAsync(int id)
    {
        return await _context.ImageGroups
            .Include(g => g.Images)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<ImageGroup>> GetByQueueIdAsync(int queueId)
    {
        return await _context.ImageGroups
            .Where(g => g.QueueId == queueId)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ImageGroup?> GetByNameAsync(int queueId, string groupName)
    {
        return await _context.ImageGroups
            .FirstOrDefaultAsync(g => g.QueueId == queueId && g.GroupName == groupName);
    }

    public async Task<int> GetCompletedCountAsync(int queueId)
    {
        return await _context.ImageGroups
            .CountAsync(g => g.QueueId == queueId && g.IsCompleted);
    }
}

public class ImageRepository : Repository<Image>, IImageRepository
{
    public ImageRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Image>> GetByQueueIdAsync(int queueId)
    {
        return await _context.Images
            .Where(i => i.QueueId == queueId)
            .OrderBy(i => i.ImageGroupId)
            .ThenBy(i => i.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Image>> GetByImageGroupIdAsync(int imageGroupId)
    {
        return await _context.Images
            .Where(i => i.ImageGroupId == imageGroupId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Image?> GetByFileHashAsync(string fileHash)
    {
        return await _context.Images
            .FirstOrDefaultAsync(i => i.FileHash == fileHash);
    }

    public async Task<Image?> GetByQueueAndHashAsync(int queueId, string fileHash)
    {
        return await _context.Images
            .FirstOrDefaultAsync(i => i.QueueId == queueId && i.FileHash == fileHash);
    }

    public async Task<HashSet<string>> GetHashesByQueueAsync(int queueId, IEnumerable<string> hashes)
    {
        var hashList = hashes.Distinct().ToList();
        var existing = await _context.Images
            .Where(i => i.QueueId == queueId && hashList.Contains(i.FileHash!))
            .Select(i => i.FileHash!)
            .ToListAsync();

        return existing.ToHashSet();
    }

    public async Task<Image?> GetByQueueAndFileNameAsync(int queueId, string fileName)
    {
        return await _context.Images
            .FirstOrDefaultAsync(i => i.QueueId == queueId && i.FileName == fileName);
    }

    public async Task<HashSet<string>> GetFileNamesByQueueAsync(int queueId, IEnumerable<string> fileNames)
    {
        var nameList = fileNames.Distinct().ToList();
        var existing = await _context.Images
            .Where(i => i.QueueId == queueId && nameList.Contains(i.FileName))
            .Select(i => i.FileName)
            .ToListAsync();

        return existing.ToHashSet();
    }

    public async Task<Image?> GetByQueueFolderAndFileNameAsync(int queueId, string folderName, string fileName)
    {
        return await _context.Images
            .FirstOrDefaultAsync(i => i.QueueId == queueId && i.FolderName == folderName && i.FileName == fileName);
    }

    public async Task<HashSet<string>> GetFolderFileKeysByQueueAsync(int queueId, IEnumerable<string> folderFileKeys)
    {
        var keyList = folderFileKeys.Distinct().ToList();
        var existing = await _context.Images
            .Where(i => i.QueueId == queueId && keyList.Contains(i.FolderName + "|" + i.FileName))
            .Select(i => i.FolderName + "|" + i.FileName)
            .ToListAsync();

        return existing.ToHashSet();
    }

    public async Task<int> GetTotalSizeByQueueIdAsync(int queueId)
    {
        return await _context.Images
            .Where(i => i.QueueId == queueId)
            .SumAsync(i => (int)i.FileSize);
    }
}

public class SelectionRecordRepository : Repository<SelectionRecord>, ISelectionRecordRepository
{
    public SelectionRecordRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SelectionRecord>> GetByQueueIdAsync(int queueId)
    {
        return await _context.SelectionRecords
            .Where(s => s.QueueId == queueId)
            .Include(s => s.User)
            .Include(s => s.SelectedImage)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SelectionRecord>> GetByUserIdAsync(int userId)
    {
        return await _context.SelectionRecords
            .Where(s => s.UserId == userId)
            .Include(s => s.Queue)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SelectionRecord?> GetByUserAndGroupAsync(int userId, int imageGroupId)
    {
        return await _context.SelectionRecords
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ImageGroupId == imageGroupId);
    }

    public async Task<int> GetCompletedGroupCountAsync(int userId, int queueId)
    {
        return await _context.SelectionRecords
            .Where(s => s.UserId == userId && s.QueueId == queueId)
            .Select(s => s.ImageGroupId)
            .Distinct()
            .CountAsync();
    }
}

public class UserProgressRepository : Repository<UserProgress>, IUserProgressRepository
{
    public UserProgressRepository(AppDbContext context) : base(context) { }

    public async Task<UserProgress?> GetByUserAndQueueAsync(int userId, int queueId)
    {
        return await _context.UserProgresses
            .FirstOrDefaultAsync(up => up.UserId == userId && up.QueueId == queueId);
    }

    public async Task<IEnumerable<UserProgress>> GetByQueueIdAsync(int queueId)
    {
        return await _context.UserProgresses
            .Where(up => up.QueueId == queueId)
            .Include(up => up.User)
            .OrderByDescending(up => up.LastUpdated)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProgress>> GetByUserIdAsync(int userId)
    {
        return await _context.UserProgresses
            .Where(up => up.UserId == userId)
            .Include(up => up.Queue)
            .OrderByDescending(up => up.LastUpdated)
            .ToListAsync();
    }
}
