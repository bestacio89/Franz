namespace Franz.Contracts.DTOs;

/// <summary>Immutable request DTO for creating a product.</summary>
public record ProductCreateRequestDto(string Name, decimal Price);
