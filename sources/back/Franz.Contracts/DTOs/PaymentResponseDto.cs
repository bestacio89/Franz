namespace Franz.Contracts.DTOs;

/// <summary>
/// Immutable response DTO for a processed payment.
/// </summary>
public record PaymentResponseDto(
    string PaymentId,
    string Status,
    string Message
);
