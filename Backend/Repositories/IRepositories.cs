using Backend.Models;

namespace Backend.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetWithQueuesAsync(int id);
    Task<IEnumerable<Project>> GetByCreatorIdAsync(int creatorId);
}

public interface IQueueRepository : IRepository<Queue>
{
    Task<Queue?> GetWithImagesAsync(int id);
    Task<IEnumerable<Queue>> GetByProjectIdAsync(int projectId);
    Task<IEnumerable<Queue>> GetByStatusAsync(string status);
}

public interface IImageGroupRepository : IRepository<ImageGroup>
{
    Task<ImageGroup?> GetWithImagesAsync(int id);
    Task<IEnumerable<ImageGroup>> GetByQueueIdAsync(int queueId);
    Task<ImageGroup?> GetByNameAsync(int queueId, string groupName);
    Task<int> GetCompletedCountAsync(int queueId);
}

public interface IImageRepository : IRepository<Image>
{
    Task<IEnumerable<Image>> GetByQueueIdAsync(int queueId);
    Task<IEnumerable<Image>> GetByImageGroupIdAsync(int imageGroupId);
    Task<Image?> GetByFileHashAsync(string fileHash);
    Task<Image?> GetByQueueAndHashAsync(int queueId, string fileHash);
    Task<HashSet<string>> GetHashesByQueueAsync(int queueId, IEnumerable<string> hashes);
    Task<Image?> GetByQueueAndFileNameAsync(int queueId, string fileName);
    Task<HashSet<string>> GetFileNamesByQueueAsync(int queueId, IEnumerable<string> fileNames);
    Task<Image?> GetByQueueFolderAndFileNameAsync(int queueId, string folderName, string fileName);
    Task<HashSet<string>> GetFolderFileKeysByQueueAsync(int queueId, IEnumerable<string> folderFileKeys);
    Task<int> GetTotalSizeByQueueIdAsync(int queueId);
}

public interface ISelectionRecordRepository : IRepository<SelectionRecord>
{
    Task<IEnumerable<SelectionRecord>> GetByQueueIdAsync(int queueId);
    Task<IEnumerable<SelectionRecord>> GetByUserIdAsync(int userId);
    Task<SelectionRecord?> GetByUserAndGroupAsync(int userId, int imageGroupId);
    Task<int> GetCompletedGroupCountAsync(int userId, int queueId);
}

public interface IUserProgressRepository : IRepository<UserProgress>
{
    Task<UserProgress?> GetByUserAndQueueAsync(int userId, int queueId);
    Task<IEnumerable<UserProgress>> GetByQueueIdAsync(int queueId);
    Task<IEnumerable<UserProgress>> GetByUserIdAsync(int userId);
}
