using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;

namespace GroundControlApp.Data.Services;

public sealed class IngredientService : GroundControlApiServiceBase, IIngredientService
{
    public IngredientService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyCollection<IngredientDto>> GetIngredientsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<IngredientDto>("api/v1/ingredients", cancellationToken);
    }

    public Task<IngredientDto> CreateIngredientAsync(
        SaveIngredientDto ingredient,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<SaveIngredientDto, IngredientDto>("api/v1/ingredients", ingredient, null, cancellationToken);
    }

    public Task<IngredientDto> UpdateIngredientAsync(
        Guid id,
        SaveIngredientDto ingredient,
        CancellationToken cancellationToken = default)
    {
        return PutAsync<SaveIngredientDto, IngredientDto>($"api/v1/ingredients/{id}", ingredient, cancellationToken);
    }
}
