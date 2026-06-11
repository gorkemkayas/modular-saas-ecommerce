using MediatR;
using Shipment.Application.Abstractions;
using Shipment.Application.Exceptions;
using Shipment.Domain.Entities;
using Shipment.Domain.Repositories;
using Subscription.Contracts;

namespace Shipment.Application.ShippingCarriers.Commands.CreateShippingCarrier;

public sealed class CreateShippingCarrierCommandHandler : IRequestHandler<CreateShippingCarrierCommand, Guid>
{
    private readonly IShippingCarrierRepository _shippingCarrierRepository;
    private readonly ISubscriptionModuleApi _subscriptionModuleApi;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShippingCarrierCommandHandler(
        IShippingCarrierRepository shippingCarrierRepository,
        ISubscriptionModuleApi subscriptionModuleApi,
        IUnitOfWork unitOfWork)
    {
        _shippingCarrierRepository = shippingCarrierRepository;
        _subscriptionModuleApi = subscriptionModuleApi;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateShippingCarrierCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new ShipmentValidationException("StoreId is required.");

        var normalizedCode = ShippingCarrier.NormalizeCode(command.Code);

        if (await _shippingCarrierRepository.ExistsByCodeAsync(command.StoreId, normalizedCode, null, cancellationToken))
            throw new DuplicateShippingCarrierCodeException(normalizedCode);

        await EnsureShippingCarrierQuotaAsync(command.StoreId, cancellationToken);

        var carrier = ShippingCarrier.Create(
            command.StoreId,
            normalizedCode,
            command.Name,
            command.ServiceCode,
            command.ServiceName,
            command.TrackingUrl,
            command.SortOrder);

        await _shippingCarrierRepository.AddAsync(carrier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return carrier.Id;
    }

    private async Task EnsureShippingCarrierQuotaAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var quota = await _subscriptionModuleApi.GetQuotaAsync(
            new QuotaRequest(storeId, SubscriptionQuotaKeys.ShippingCarriers),
            cancellationToken);

        if (quota is null)
            throw new ShipmentValidationException("Shipping carrier quota is not configured for this tenant.");

        if (!quota.Limit.HasValue)
            return;

        var currentCount = await _shippingCarrierRepository.CountActiveByStoreIdAsync(
            storeId,
            cancellationToken);

        if (currentCount >= quota.Limit.Value)
        {
            throw new ShippingCarrierQuotaExceededException(
                storeId,
                SubscriptionQuotaKeys.ShippingCarriers,
                currentCount,
                quota.Limit.Value);
        }
    }
}
