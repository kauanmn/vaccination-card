using Domain.Entities;

namespace Application.Ports.Persistence.Repositories;

public interface IVaccinationRepository
{
    Task CreateAsync(Vaccination vaccination);
    Task UpdateAsync(Vaccination vaccination);
    Task SoftDeleteAsync(Guid id);
    Task<Vaccination?> GetByIdAsync(Guid id);
}
