using Domain.Entities;
using Application.Ports.Persistence.Repositories;
using Infrastructure.Persistence.SQLite.Client;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLite.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly SqliteContext _context;

    public PatientRepository(SqliteContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var patient = await _context.Patients
            .Include(p => p.Vaccinations)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return;

        _context.Entry(patient).Property("IsDeleted").CurrentValue = true;
        foreach (var vaccination in patient.Vaccinations)
            _context.Entry(vaccination).Property("IsDeleted").CurrentValue = true;

        await _context.SaveChangesAsync();
    }

    public async Task<Patient?> GetByIdAsync(Guid id)
    {
        return await _context.Patients
            .Include(p => p.Vaccinations)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient?> GetByUsernameAsync(string username)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(p => p.Username == username);
    }

    public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> ListAsync(int page, int pageSize)
    {
        var query = _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.Name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}