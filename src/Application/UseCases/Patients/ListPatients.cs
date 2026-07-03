using Application.Dtos.Common;
using Application.Dtos.Patients;
using Application.Ports.Persistence.Repositories;

namespace Application.UseCases.Patients;

public class ListPatients
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IPatientRepository _patientRepository;

    public ListPatients(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PagedResponse<PatientResponse>> RunAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var (items, totalCount) = await _patientRepository.ListAsync(page, pageSize);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<PatientResponse>
        {
            Items = items.Select(patient => patient.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
