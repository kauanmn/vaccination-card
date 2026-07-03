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
        var patient = await _context.Patients.FindAsync(id);
        if (patient is null)
            return; // TODO: lançar exceção de domínio

        _context.Entry(patient).Property("IsDeleted").CurrentValue = true;
        await _context.SaveChangesAsync();
    }

    public async Task<Patient?> GetByIdAsync(Guid id)
    {
        return await _context.Patients
            .Include(p => p.Vaccinations)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}