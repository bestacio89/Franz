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
