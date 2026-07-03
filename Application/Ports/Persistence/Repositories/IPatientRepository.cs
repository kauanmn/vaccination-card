using Domain.Entities;

namespace Application.Ports.Persistence.Repositories;

public interface IPatientRepository
{
    Task CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task SoftDeleteAsync(Guid id);
    Task<Patient?> GetByIdAsync(Guid id);
}