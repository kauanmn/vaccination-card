using Application.Dtos.Common;
using Application.Dtos.Vaccines;
using Application.Ports.Persistence.Repositories;

namespace Application.UseCases.Vaccines;

public class ListVaccines
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IVaccineRepository _vaccineRepository;

    public ListVaccines(IVaccineRepository vaccineRepository)
    {
        _vaccineRepository = vaccineRepository;
    }

    public async Task<PagedResponse<VaccineResponse>> RunAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var (items, totalCount) = await _vaccineRepository.ListAsync(page, pageSize);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<VaccineResponse>
        {
            Items = items.Select(vaccine => vaccine.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
