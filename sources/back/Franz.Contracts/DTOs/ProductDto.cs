namespace Franz.Contracts.DTOs;

/// <summary>Immutable DTO representing a product in the system.</summary>
public record ProductDto(int Id, string Name, decimal Price);
