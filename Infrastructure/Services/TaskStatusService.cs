using AutoMapper;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services;

public class TaskStatusService(Sen4Context db, IMapper mapper): ITaskStatusService
{
    public async Task<List<TaskStatusReadDTO>> List()
    {
        var statuses = await db.TaskStatuses.ToListAsync();
        return mapper.Map<List<TaskStatusReadDTO>>(statuses);
    }
}