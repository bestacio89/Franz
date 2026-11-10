namespace Franz.Contracts.DTOs;

/// <summary>Immutable request DTO for updating a product.</summary>
public record ProductUpdateRequestDto(string Name, decimal Price);
