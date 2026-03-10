using MediatR;
using Store.Domain.Stores;
using Store.Domain.ValueObjects;

namespace Store.Application.Stores.Queries.CheckStoreSlugAvailability
{
    public sealed class CheckStoreSlugAvailabilityQueryHandler : IRequestHandler<CheckStoreSlugAvailabilityQuery, bool>
    {
        private readonly IStoreRepository _storeRepository;
        public CheckStoreSlugAvailabilityQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }
        public async Task<bool> Handle(CheckStoreSlugAvailabilityQuery request, CancellationToken cancellationToken)
        {
            var slug = Slug.Create(request.Slug);
            return !await _storeRepository.ExistsBySlugAsync(slug, cancellationToken);
        }
    }
}
