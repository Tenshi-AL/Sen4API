using AutoMapper;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services;

public class PriorityService(Sen4Context db, IMapper mapper): IPriorityService
{
    public async Task<List<PriorityReadDTO>> List()
    {
        var list = await db.Priorities.ToListAsync();
        return mapper.Map<List<PriorityReadDTO>>(list);
    }
}