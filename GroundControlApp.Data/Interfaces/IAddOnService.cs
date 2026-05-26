using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IAddOnService
{
    Task<IReadOnlyCollection<AddOnDto>> GetAddOnsAsync(CancellationToken cancellationToken = default);
}
