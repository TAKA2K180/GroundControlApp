using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IIngredientService
{
    Task<IReadOnlyCollection<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default);
}
