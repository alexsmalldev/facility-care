namespace FacilityCare.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(int serviceRequestId);
    Task ConfirmPaymentAsync(string paymentIntentId);
    Task RefundPaymentAsync(int serviceRequestId);
    Task<string> CreatePaymentIntentForServiceAsync(int serviceTypeId);
}