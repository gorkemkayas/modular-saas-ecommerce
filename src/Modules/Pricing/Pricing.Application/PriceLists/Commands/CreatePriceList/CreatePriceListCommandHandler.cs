using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Application.Abstractions;
using Pricing.Application.Exceptions;
using Pricing.Domain.Entities;
using Pricing.Domain.Repositories;
using Pricing.Domain.ValueObjects;
using Subscription.Contracts;

namespace Pricing.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, Guid>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly ISubscriptionModuleApi _subscriptionModuleApi;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePriceListCommandHandler> _logger;

    public CreatePriceListCommandHandler(
        IPriceListRepository priceListRepository,
        ISubscriptionModuleApi subscriptionModuleApi,
        IUnitOfWork unitOfWork,
        ILogger<CreatePriceListCommandHandler> logger)
    {
        _priceListRepository = priceListRepository;
        _subscriptionModuleApi = subscriptionModuleApi;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreatePriceListCommand command, CancellationToken cancellationToken)
    {
        if (command.StoreId == Guid.Empty)
            throw new PricingValidationException("StoreId is required.");

        var currency = Currency.Create(command.CurrencyCode);

        if (command.IsDefault &&
            await _priceListRepository.ExistsDefaultActiveListAsync(command.StoreId, currency, cancellationToken: cancellationToken))
        {
            throw new DuplicateDefaultPriceListException(command.StoreId, currency.Code);
        }

        await EnsurePriceListQuotaAsync(command.StoreId, cancellationToken);

        var priceList = PriceList.Create(command.StoreId, command.Name, currency, command.Priority, command.IsDefault);
        await _priceListRepository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Pricing price list created | PriceListId: {PriceListId} | StoreId: {StoreId} | Currency: {Currency} | IsDefault: {IsDefault}",
            priceList.Id,
            command.StoreId,
            currency.Code,
            command.IsDefault);

        return priceList.Id;
    }

    private async Task EnsurePriceListQuotaAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var quota = await _subscriptionModuleApi.GetQuotaAsync(
            new QuotaRequest(storeId, SubscriptionQuotaKeys.PricingPriceLists),
            cancellationToken);

        if (quota is null)
            throw new PricingValidationException("Price list quota is not configured for this tenant.");

        if (!quota.Limit.HasValue)
            return;

        var currentCount = await _priceListRepository.CountNonArchivedByStoreIdAsync(
            storeId,
            cancellationToken);

        if (currentCount >= quota.Limit.Value)
        {
            throw new PriceListQuotaExceededException(
                storeId,
                SubscriptionQuotaKeys.PricingPriceLists,
                currentCount,
                quota.Limit.Value);
        }
    }
}
