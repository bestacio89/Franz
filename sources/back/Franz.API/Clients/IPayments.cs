using Refit;
using System.Threading.Tasks;

namespace Franz.API.Clients
{
  public interface IPaymentsClient
  {
    [Post("/payments")]
    Task<PaymentResponse> ProcessPaymentAsync([Body] PaymentRequest request);

    [Get("/payments/{id}")]
    Task<PaymentResponse> GetPaymentStatusAsync(string id);
  }

  // DTOs
  public record PaymentRequest(string OrderId, decimal Amount, string Currency, string PaymentMethod);
  public record PaymentResponse(string PaymentId, string Status, string Message);
}
