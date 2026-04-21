using MediatR;

namespace Pricing.Application.PriceLists.Commands.ArchivePriceList;

public sealed record ArchivePriceListCommand(Guid StoreId, Guid PriceListId) : IRequest;
