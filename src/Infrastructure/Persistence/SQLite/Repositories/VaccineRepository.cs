using Domain.Entities;
using Application.Ports.Persistence.Repositories;
using Infrastructure.Persistence.SQLite.Client;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLite.Repositories;

public class VaccineRepository : IVaccineRepository
{
    private readonly SqliteContext _context;

    public VaccineRepository(SqliteContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Vaccine vaccine)
    {
        await _context.Vaccines.AddAsync(vaccine);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vaccine vaccine)
    {
        _context.Vaccines.Update(vaccine);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var vaccine = await _context.Vaccines.FindAsync(id);
        if (vaccine is null)
            return;

        _context.Entry(vaccine).Property("IsDeleted").CurrentValue = true;
        await _context.SaveChangesAsync();
    }

    public async Task<Vaccine?> GetByIdAsync(Guid id)
    {
        return await _context.Vaccines.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<(IReadOnlyList<Vaccine> Items, int TotalCount)> ListAsync(int page, int pageSize)
    {
        var query = _context.Vaccines
            .AsNoTracking()
            .OrderBy(v => v.Name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}