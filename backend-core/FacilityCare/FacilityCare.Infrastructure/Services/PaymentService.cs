using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace FacilityCare.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreatePaymentIntentAsync(int serviceRequestId)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .FirstOrDefaultAsync(sr => sr.Id == serviceRequestId);

        if (serviceRequest == null)
            throw new Exception("Service request not found");

        if (!serviceRequest.ServiceType.IsPaid)
            throw new Exception("This service does not require payment");

        if (serviceRequest.PaymentStatus == PaymentStatus.Paid)
            throw new Exception("This service request has already been paid");

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(serviceRequest.ServiceType.Price!.Value * 100),
            Currency = "gbp",
            Metadata = new Dictionary<string, string>
            {
                { "serviceRequestId", serviceRequestId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        serviceRequest.PaymentIntentId = paymentIntent.Id;
        await _context.SaveChangesAsync();

        return paymentIntent.ClientSecret;
    }

    public async Task ConfirmPaymentAsync(string paymentIntentId)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.PaymentIntentId == paymentIntentId);

        if (serviceRequest == null)
            throw new Exception("Service request not found");

        serviceRequest.PaymentStatus = PaymentStatus.Paid;
        await _context.SaveChangesAsync();
    }

    public async Task RefundPaymentAsync(int serviceRequestId)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == serviceRequestId);

        if (serviceRequest == null)
            throw new Exception("Service request not found");

        if (serviceRequest.PaymentStatus != PaymentStatus.Paid)
            throw new Exception("This service request has not been paid");

        if (string.IsNullOrEmpty(serviceRequest.PaymentIntentId))
            throw new Exception("No payment intent found for this service request");

        var refundOptions = new RefundCreateOptions
        {
            PaymentIntent = serviceRequest.PaymentIntentId
        };

        var refundService = new RefundService();
        await refundService.CreateAsync(refundOptions);

        serviceRequest.PaymentStatus = PaymentStatus.Refunded;
        await _context.SaveChangesAsync();
    }

    public async Task<string> CreatePaymentIntentForServiceAsync(int serviceTypeId)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(serviceTypeId);
        if (serviceType == null)
            throw new Exception("Service type not found");

        if (!serviceType.IsPaid)
            throw new Exception("This service does not require payment");

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(serviceType.Price!.Value * 100),
            Currency = "gbp",
            Metadata = new Dictionary<string, string>
        {
            { "serviceTypeId", serviceTypeId.ToString() }
        }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);
        return paymentIntent.ClientSecret;
    }
}