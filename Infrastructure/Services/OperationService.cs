using AutoMapper;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services;

public class OperationService(Sen4Context db, IMapper mapper): IOperationService
{
    public async Task<List<OperationReadDTO>> List()
    {
        var operations = await db.Operations.ToListAsync();
        return mapper.Map<List<OperationReadDTO>>(operations);
    }
}