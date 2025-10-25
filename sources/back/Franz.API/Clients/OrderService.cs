


using Franz.Contracts.DTOs;
using Franz.Contracts.Infrastructure;

public class OrderService
{
  private readonly ICatalogClient _catalog;
  private readonly IPaymentsClient _payments;

  public OrderService(ICatalogClient catalog, IPaymentsClient payments)
  {
    _catalog = catalog;
    _payments = payments;
  }

  public async Task PlaceOrder(int productId)
  {
    var product = await _catalog.GetProductByIdAsync(productId);

    var payment = await _payments.ProcessPaymentAsync(
        new PaymentRequestDto(Guid.NewGuid().ToString(), product.Price, "EUR", "CreditCard")
    );

    if (payment.Status != "Success")
      throw new Exception($"Payment failed: {payment.Message}");
  }
}
