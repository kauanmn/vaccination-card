using Domain.Entities;

namespace Application.Ports.Persistence.Repositories;

public interface IVaccineRepository
{
    Task CreateAsync(Vaccine vaccine);
    Task UpdateAsync(Vaccine vaccine);
    Task SoftDeleteAsync(Guid id);
    Task<Vaccine?> GetByIdAsync(Guid id);
}
