using AutoMapper;
using Domain.Models;
using Infrastructure.DTO;
using Infrastructure.Extension;
using Infrastructure.Interfaces;
using Infrastructure.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services;

public class ProjectTaskService(Sen4Context db, IMapper mapper): IProjectTaskService
{
    public async Task<ProjectTaskReadDTO?> Create(ProjectTaskWriteDTO projectWriteDto)
    {
        if (await db.UsersProjects.FirstOrDefaultAsync(p => p.UserId == projectWriteDto.UserCreatedId
                                                            || p.UserId == projectWriteDto.UserExecutorId) is null)
            return null;
        
        var task = mapper.Map<ProjectTask>(projectWriteDto);
        db.ProjectTasks.Add(task);
        await db.SaveChangesAsync();

        return mapper.Map<ProjectTaskReadDTO>(task);
    }

    public async Task<ProjectTaskReadDTO> Get(Guid id)
    {
        var result = await db.ProjectTasks
            .Include(p=>p.TaskStatus)
            .Include(p=>p.Priority)
            .FirstOrDefaultAsync(p=>p.Id == id);
        return mapper.Map<ProjectTaskReadDTO>(result);
    }

    public async Task<ProjectTaskReadDTO?> Update(Guid id, ProjectTaskWriteDTO projectTask)
    {
        var task = await db.ProjectTasks.FindAsync(id);
        if (task is null) return null;

        task.Name = projectTask.Name;
        task.Description = projectTask.Description;
        task.TaskStatusId = projectTask.TaskStatusId;
        task.CreatedDate = projectTask.CreatedDate;
        task.DeadlineDate = projectTask.DeadlineDate;
        task.UserExecutorId = projectTask.UserExecutorId;

        db.ProjectTasks.Update(task);
        await db.SaveChangesAsync();
        return mapper.Map<ProjectTaskReadDTO>(task);
    }

    public async Task<ProjectTaskReadDTO?> Patch(Guid id, JsonPatchDocument<ProjectTaskWriteDTO> projectTask)
    {
        var task = await db.ProjectTasks.FindAsync(id);
        if (task is null) return null;
     
        var taskPatchDocument = mapper.Map<JsonPatchDocument<ProjectTask>>(projectTask);
        taskPatchDocument.ApplyTo(task);

        db.ProjectTasks.Update(task);
        await db.SaveChangesAsync();

        return mapper.Map<ProjectTaskReadDTO>(task);
    }

    public async Task Delete(Guid id)
    {
        var projectTask = await db.ProjectTasks.FindAsync(id);
        if (projectTask is not null)
        {
            db.ProjectTasks.Remove(projectTask);
            await db.SaveChangesAsync();
        }
    }

    public async Task<PaginatedList<ProjectTaskReadDTO>> List(ProjectTaskListRequest projectTaskListRequest)
    {
        var filteredQuery = db.ProjectTasks
            .Include(p => p.TaskStatus)
            .Include(p => p.UserCreated)
            .Include(p => p.UserExecutor)
            .Include(p => p.Project)
            .Filtered(projectTaskListRequest); 
        
        var taskTotalCount = await filteredQuery.CountAsync();

        var list = await filteredQuery
            .Sort(projectTaskListRequest.SortProperty, projectTaskListRequest.SortByDescending)
            .Pagination(projectTaskListRequest.PageSize, projectTaskListRequest.PageNumber)
            .ToListAsync();
        
        var mappedTasks = mapper.Map<List<ProjectTaskReadDTO>>(list);
        int? totalPage = (projectTaskListRequest.PageNumber == null || projectTaskListRequest.PageSize == null)
            ? null
            : (int)Math.Ceiling(taskTotalCount / (double)projectTaskListRequest.PageSize);
        
        return new PaginatedList<ProjectTaskReadDTO>(
            list: mappedTasks,
            pageIndex: projectTaskListRequest.PageNumber,
            totalPages: totalPage);
    }
}