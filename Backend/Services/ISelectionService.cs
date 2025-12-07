using Backend.DTOs;

namespace Backend.Services;

/// <summary>
/// 选择记录服务接口
/// </summary>
public interface ISelectionService
{
    /// <summary>
    /// 创建选择记录
    /// </summary>
    Task<SelectionDto> CreateAsync(CreateSelectionDto createDto, int userId);

    /// <summary>
    /// 根据ID获取选择记录
    /// </summary>
    Task<SelectionDto?> GetByIdAsync(int id);

    /// <summary>
    /// 获取队列的所有选择记录
    /// </summary>
    Task<IEnumerable<SelectionDto>> GetByQueueIdAsync(int queueId, int? userId = null, bool isAdmin = false);

    /// <summary>
    /// 获取用户进度
    /// </summary>
    Task<UserProgressDto> GetProgressAsync(int queueId, int userId);

    /// <summary>
    /// 获取所有用户进度（仅管理员）
    /// </summary>
    Task<IEnumerable<UserProgressDto>> GetAllProgressAsync(int? queueId = null);
}

