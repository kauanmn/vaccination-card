using Application.Ports.Persistence.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.SQLite.Client;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLite.Repositories;

public class VaccinationRepository : IVaccinationRepository
{
    private readonly SqliteContext _context;

    public VaccinationRepository(SqliteContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Vaccination vaccination)
    {
        await _context.Vaccinations.AddAsync(vaccination);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vaccination vaccination)
    {
        _context.Vaccinations.Update(vaccination);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var vaccination = await _context.Vaccinations.FindAsync(id);
        if (vaccination is null)
            return;

        _context.Entry(vaccination).Property("IsDeleted").CurrentValue = true;
        await _context.SaveChangesAsync();
    }

    public async Task<Vaccination?> GetByIdAsync(Guid id)
    {
        return await _context.Vaccinations.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<int?> GetMaxDoseByVaccineAsync(Guid vaccineId)
    {
        return await _context.Vaccinations
            .Where(v => v.VaccineId == vaccineId)
            .MaxAsync(v => (int?)v.Dose);
    }
}