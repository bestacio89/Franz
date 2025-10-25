namespace Franz.Contracts.DTOs;

/// <summary>
/// Immutable request DTO for initiating a payment.
/// </summary>
public record PaymentRequestDto(
    string OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod
);

/// <summary>
/// Immutable response DTO for a processed payment.
/// </summary>
public record PaymentResponseDto(
    string PaymentId,
    string Status,
    string Message
);
