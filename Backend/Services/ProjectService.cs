using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// 项目服务实现
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync()
    {
        // 直接使用数据库查询，一次性加载所有需要的导航属性
        var projects = await _unitOfWork.Projects.GetPagedAsync(
            pageNumber: 1,
            pageSize: int.MaxValue,
            includeProperties: "CreatedBy,Queues",
            orderBy: q => q.OrderByDescending(p => p.CreatedAt)
        );

        return projects.Items.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedById = p.CreatedById,
            CreatedByUsername = p.CreatedBy?.Username ?? string.Empty,
            CreatedAt = p.CreatedAt,
            QueueCount = p.Queues?.Count ?? 0
        });
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var project = await _unitOfWork.Projects.GetWithQueuesAsync(id);
        if (project == null)
        {
            return null;
        }

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedById = project.CreatedById,
            CreatedByUsername = project.CreatedBy.Username,
            CreatedAt = project.CreatedAt,
            QueueCount = project.Queues.Count
        };
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto createDto, int userId)
    {
        var project = new Project
        {
            Name = createDto.Name,
            Description = createDto.Description,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // 重新加载以获取导航属性
        var createdProject = await _unitOfWork.Projects.GetWithQueuesAsync(project.Id);
        if (createdProject == null)
        {
            throw new InvalidOperationException("项目创建失败");
        }

        return new ProjectDto
        {
            Id = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            CreatedById = createdProject.CreatedById,
            CreatedByUsername = createdProject.CreatedBy.Username,
            CreatedAt = createdProject.CreatedAt,
            QueueCount = createdProject.Queues.Count
        };
    }

    public async Task<ProjectDto?> UpdateAsync(int id, UpdateProjectDto updateDto)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return null;
        }

        project.Name = updateDto.Name;
        project.Description = updateDto.Description;
        project.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync();

        // 重新加载以获取导航属性
        var updatedProject = await _unitOfWork.Projects.GetWithQueuesAsync(id);
        if (updatedProject == null)
        {
            return null;
        }

        return new ProjectDto
        {
            Id = updatedProject.Id,
            Name = updatedProject.Name,
            Description = updatedProject.Description,
            CreatedById = updatedProject.CreatedById,
            CreatedByUsername = updatedProject.CreatedBy.Username,
            CreatedAt = updatedProject.CreatedAt,
            QueueCount = updatedProject.Queues.Count
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null)
        {
            return false;
        }

        // 使用软删除
        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

