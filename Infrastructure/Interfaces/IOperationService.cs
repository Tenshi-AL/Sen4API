using Infrastructure.DTO;

namespace Infrastructure.Interfaces;

public interface IOperationService
{
    Task<List<OperationReadDTO>> List();
}