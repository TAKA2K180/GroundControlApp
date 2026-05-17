using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Data.Interfaces;

public interface IIngredientService
{
    Task<IReadOnlyCollection<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default);

    Task<IngredientDto> CreateIngredientAsync(SaveIngredientDto ingredient, CancellationToken cancellationToken = default);

    Task<IngredientDto> UpdateIngredientAsync(Guid id, SaveIngredientDto ingredient, CancellationToken cancellationToken = default);
}
