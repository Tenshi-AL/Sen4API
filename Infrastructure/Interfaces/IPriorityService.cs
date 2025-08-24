using Infrastructure.DTO;

namespace Infrastructure.Interfaces;

public interface IPriorityService
{
     Task<List<PriorityReadDTO>> List();
}