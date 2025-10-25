namespace Franz.Contracts.DTOs;

/// <summary>
/// Immutable DTO for Member entities, compatible with
/// Franz.Common.Mapping v1.6.18 (constructor-aware mapping).
/// </summary>
public record MemberDto(
    int Id,
    string FullName,
    string Email,
    int BorrowedBooksCount
);
