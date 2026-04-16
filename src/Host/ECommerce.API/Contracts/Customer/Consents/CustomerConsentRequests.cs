namespace ECommerce.API.Contracts.Customer.Consents;

public sealed record UpdateCustomerConsentRequest(
    bool IsGranted,
    string Source);
