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
}
