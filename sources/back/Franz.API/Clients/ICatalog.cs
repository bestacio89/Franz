

using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Franz.API.Clients;

  public interface ICatalogClient
  {
    [Get("/products")]
    Task<IEnumerable<ProductDto>> GetProductsAsync();

    [Get("/products/{id}")]
    Task<ProductDto> GetProductByIdAsync(int id);

    [Post("/products")]
    Task<ProductDto> CreateProductAsync([Body] ProductCreateRequest request);

    [Put("/products/{id}")]
    Task<ProductDto> UpdateProductAsync(int id, [Body] ProductUpdateRequest request);

    [Delete("/products/{id}")]
    Task DeleteProductAsync(int id);
  }

  // DTOs can live in a shared "Contracts" or "Dtos" folder
  public record ProductDto(int Id, string Name, decimal Price);
  public record ProductCreateRequest(string Name, decimal Price);
  public record ProductUpdateRequest(string Name, decimal Price);

