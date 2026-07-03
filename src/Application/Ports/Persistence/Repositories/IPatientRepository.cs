using Domain.Entities;

namespace Application.Ports.Persistence.Repositories;

public interface IPatientRepository
{
    Task CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task SoftDeleteAsync(Guid id);
    Task<Patient?> GetByIdAsync(Guid id);
    Task<Patient?> GetByUsernameAsync(string username);
    Task<(IReadOnlyList<Patient> Items, int TotalCount)> ListAsync(int page, int pageSize);
}