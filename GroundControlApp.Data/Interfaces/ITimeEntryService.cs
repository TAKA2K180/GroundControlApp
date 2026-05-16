using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface ITimeEntryService
{
    Task<IReadOnlyCollection<TimeEntryDto>> GetTimeEntriesAsync(CancellationToken cancellationToken = default);
}
