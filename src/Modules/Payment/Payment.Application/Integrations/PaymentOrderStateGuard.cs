using Payment.Application.Exceptions;

namespace Payment.Application.Integrations;

internal static class PaymentOrderStateGuard
{
    public static void EnsureCanCreatePayment(OrderPaymentContext orderContext)
    {
        EnsureOrderIsPayable(orderContext, "created");

        if (orderContext.PaymentStatus is OrderPaymentLifecycleStatus.Authorized or OrderPaymentLifecycleStatus.Captured or OrderPaymentLifecycleStatus.Refunded)
        {
            throw new PaymentValidationException(
                $"Payment cannot be created because the order payment status is already {orderContext.PaymentStatus}.");
        }
    }

    public static void EnsureCanAuthorizePayment(OrderPaymentContext orderContext)
    {
        EnsureOrderIsPayable(orderContext, "authorized");

        if (orderContext.PaymentStatus is OrderPaymentLifecycleStatus.Authorized or OrderPaymentLifecycleStatus.Captured or OrderPaymentLifecycleStatus.Refunded)
        {
            throw new PaymentValidationException(
                $"Payment cannot be authorized because the order payment status is already {orderContext.PaymentStatus}.");
        }
    }

    public static void EnsureCanCapturePayment(OrderPaymentContext orderContext)
    {
        EnsureOrderIsPayable(orderContext, "captured");

        if (orderContext.PaymentStatus is OrderPaymentLifecycleStatus.Captured or OrderPaymentLifecycleStatus.Refunded)
        {
            throw new PaymentValidationException(
                $"Payment cannot be captured because the order payment status is already {orderContext.PaymentStatus}.");
        }
    }

    private static void EnsureOrderIsPayable(OrderPaymentContext orderContext, string operation)
    {
        if (orderContext.Status == OrderLifecycleStatus.Cancelled)
            throw new PaymentValidationException($"Payment cannot be {operation} for a cancelled order.");

        if (orderContext.Status == OrderLifecycleStatus.Completed)
            throw new PaymentValidationException($"Payment cannot be {operation} for a completed order.");

        if (orderContext.FulfillmentStatus is OrderFulfillmentLifecycleStatus.Shipped or OrderFulfillmentLifecycleStatus.Delivered or OrderFulfillmentLifecycleStatus.Returned)
        {
            throw new PaymentValidationException(
                $"Payment cannot be {operation} after the order fulfillment status becomes {orderContext.FulfillmentStatus}.");
        }
    }
}
