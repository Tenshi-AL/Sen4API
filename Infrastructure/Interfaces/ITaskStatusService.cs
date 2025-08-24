using Infrastructure.DTO;

namespace Infrastructure.Interfaces;

public interface ITaskStatusService
{
    public Task<List<TaskStatusReadDTO>> List();
}