namespace ECommerce.API.Contracts.Order;

public sealed record GetMyOrdersRequest(
    int PageNumber = 1,
    int PageSize = 20);
