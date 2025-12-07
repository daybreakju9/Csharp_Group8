using Backend.Models;

namespace Backend.Repositories;

/// <summary>
/// 工作单元接口 - 协调多个仓储的事务
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    IQueueRepository Queues { get; }
    IImageGroupRepository ImageGroups { get; }
    IImageRepository Images { get; }
    ISelectionRecordRepository SelectionRecords { get; }
    IUserProgressRepository UserProgresses { get; }
    IRepository<User> Users { get; }

    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
