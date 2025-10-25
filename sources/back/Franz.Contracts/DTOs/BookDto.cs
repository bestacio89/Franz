namespace Franz.Contracts.DTOs;

/// <summary>
/// Data Transfer Object for Book entities (immutable and record-based).
/// Fully compatible with Franz.Common.Mapping v1.6.18 (constructor-aware).
/// </summary>
public record BookDto(
    int Id,
    string Title,
    string Author,
    string Isbn,
    DateTime PublishedOn,
    int CopiesAvailable
);