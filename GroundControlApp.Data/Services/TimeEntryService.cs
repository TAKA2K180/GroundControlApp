using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class TimeEntryService : GroundControlApiServiceBase, ITimeEntryService
{
    public TimeEntryService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<TimeEntryDto>> GetTimeEntriesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<TimeEntryDto>("api/v1/employee-time/time-entries", cancellationToken);
    }
}
