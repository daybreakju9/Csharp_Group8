using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Repositories;

/// <summary>
/// 工作单元实现 - 管理多个仓储和事务
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IProjectRepository Projects { get; }
    public IQueueRepository Queues { get; }
    public IImageGroupRepository ImageGroups { get; }
    public IImageRepository Images { get; }
    public ISelectionRecordRepository SelectionRecords { get; }
    public IUserProgressRepository UserProgresses { get; }
    public IRepository<User> Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Projects = new ProjectRepository(_context);
        Queues = new QueueRepository(_context);
        ImageGroups = new ImageGroupRepository(_context);
        Images = new ImageRepository(_context);
        SelectionRecords = new SelectionRecordRepository(_context);
        UserProgresses = new UserProgressRepository(_context);
        Users = new Repository<User>(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            return false; // Transaction already started
        }

        _transaction = await _context.Database.BeginTransactionAsync();
        return true;
    }

    public async Task<bool> CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            return false; // No transaction to commit
        }

        try
        {
            await _transaction.CommitAsync();
            return true;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            return; // No transaction to rollback
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
