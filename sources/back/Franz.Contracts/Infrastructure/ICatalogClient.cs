

using Franz.Common.DependencyInjection;
using Franz.Contracts.DTOs;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Franz.Contracts.Infrastructure;

  public interface ICatalogClient :IScopedDependency
  {
    [Get("/products")]
    Task<IEnumerable<ProductDto>> GetProductsAsync();

    [Get("/products/{id}")]
    Task<ProductDto> GetProductByIdAsync(int id);

    [Post("/products")]
    Task<ProductDto> CreateProductAsync([Body] ProductCreateRequestDto request);

    [Put("/products/{id}")]
    Task<ProductDto> UpdateProductAsync(int id, [Body] ProductUpdateRequestDto request);

    [Delete("/products/{id}")]
    Task DeleteProductAsync(int id);
  }

  // DTOs can live in a shared "Contracts" or "Dtos" folder

