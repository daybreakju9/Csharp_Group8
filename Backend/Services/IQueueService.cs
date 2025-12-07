using Backend.DTOs;

namespace Backend.Services;

/// <summary>
/// 队列服务接口
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// 获取所有队列
    /// </summary>
    Task<IEnumerable<QueueDto>> GetAllAsync(int? projectId = null);

    /// <summary>
    /// 根据ID获取队列
    /// </summary>
    Task<QueueDto?> GetByIdAsync(int id);

    /// <summary>
    /// 创建队列
    /// </summary>
    Task<QueueDto> CreateAsync(CreateQueueDto createDto);

    /// <summary>
    /// 更新队列
    /// </summary>
    Task<QueueDto?> UpdateAsync(int id, UpdateQueueDto updateDto);

    /// <summary>
    /// 删除队列
    /// </summary>
    Task<bool> DeleteAsync(int id);
}

