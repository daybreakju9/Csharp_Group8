using Backend.DTOs;

namespace Backend.Services;

/// <summary>
/// 项目服务接口
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 获取所有项目
    /// </summary>
    Task<IEnumerable<ProjectDto>> GetAllAsync();

    /// <summary>
    /// 根据ID获取项目
    /// </summary>
    Task<ProjectDto?> GetByIdAsync(int id);

    /// <summary>
    /// 创建项目
    /// </summary>
    Task<ProjectDto> CreateAsync(CreateProjectDto createDto, int userId);

    /// <summary>
    /// 更新项目
    /// </summary>
    Task<ProjectDto?> UpdateAsync(int id, UpdateProjectDto updateDto);

    /// <summary>
    /// 删除项目
    /// </summary>
    Task<bool> DeleteAsync(int id);
}

