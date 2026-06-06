using MediatR;
using Store.Application.Abstractions;
using Store.Application.DTOs;
using Store.Domain.Stores;

namespace Store.Application.Stores.Queries.GetStoreById
{
    public sealed class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, StoreDto?>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetStoreByIdQueryHandler(IStoreRepository repository, IUnitOfWork unitOfWork)
        {
            _storeRepository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StoreDto?> Handle(GetStoreByIdQuery query, CancellationToken cancellationToken)
        {
            var store = await _storeRepository.GetByIdAsync(query.StoreId, cancellationToken);
            if (store is null)
                return null;

            return new StoreDto(
                store.Id,
                store.TenantId,
                store.Name,
                store.Slug.Value,
                store.Description,
                store.LogoUrl,
                store.Status,
                store.IsPublished,
                store.HeroImageUrl,
                store.HeroMediaType,
                store.HeroEyebrowText,
                store.HeroTitle,
                store.HeroAccentTitle,
                store.HeroDescription,
                store.HeroPrimaryButtonText,
                store.LoginPageImageUrl,
                store.RegisterPageImageUrl);
        }
    }
}

