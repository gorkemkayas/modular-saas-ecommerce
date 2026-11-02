namespace Payment.Application.Exceptions;

public sealed class PaymentOperationInProgressException()
    : ApplicationException("Another refund operation for this payment is in progress. Retry later.") { }
