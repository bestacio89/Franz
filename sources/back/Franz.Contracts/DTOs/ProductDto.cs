namespace Franz.Contracts.DTOs;

/// <summary>
/// Immutable DTO representing a product in the system.
/// Fully compatible with Franz.Common.Mapping v1.6.18 (constructor-aware).
/// </summary>
public record ProductDto(
    int Id,
    string Name,
    decimal Price
);

/// <summary>
/// Immutable request DTO for creating a product.
/// </summary>
public record ProductCreateRequestDto(
    string Name,
    decimal Price
);

/// <summary>
/// Immutable request DTO for updating a product.
/// </summary>
public record ProductUpdateRequestDto(
    string Name,
    decimal Price
);
