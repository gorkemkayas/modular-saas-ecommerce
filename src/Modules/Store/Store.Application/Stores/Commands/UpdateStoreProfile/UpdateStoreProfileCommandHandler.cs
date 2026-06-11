using MediatR;
using Store.Application.Abstractions;
using Store.Application.Exceptions;
using Store.Domain.Stores;
using Subscription.Contracts;

namespace Store.Application.Stores.Commands.UpdateStoreProfile
{
    public sealed class UpdateStoreProfileCommandHandler : IRequestHandler<UpdateStoreProfileCommand>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ISubscriptionModuleApi? _subscriptionModuleApi;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStoreProfileCommandHandler(
            IStoreRepository storeRepository,
            IUnitOfWork unitOfWork,
            ISubscriptionModuleApi? subscriptionModuleApi = null)
        {
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
            _subscriptionModuleApi = subscriptionModuleApi;
        }

        public async Task Handle(UpdateStoreProfileCommand command, CancellationToken cancellationToken = default)
        {
            var store = await _storeRepository.GetByTenantIdAsync(command.TenantId, cancellationToken);
            if (store == null)
            {
                throw new StoreNotFoundException(command.TenantId);
            }

            if (command.HeroMediaType == StorefrontHeroMediaType.Video)
            {
                var hasFeature = _subscriptionModuleApi is not null &&
                    await _subscriptionModuleApi.HasFeatureAsync(
                    new FeatureAccessRequest(command.TenantId, SubscriptionFeatureKeys.StorefrontVideoHero),
                    cancellationToken);

                if (!hasFeature)
                    throw new StoreFeatureUnavailableException(command.TenantId, SubscriptionFeatureKeys.StorefrontVideoHero);
            }

            store.UpdateProfile(
                command.Name,
                command.Description,
                command.LogoUrl,
                command.HeroImageUrl,
                command.HeroMediaType,
                command.HeroEyebrowText,
                command.HeroTitle,
                command.HeroAccentTitle,
                command.HeroDescription,
                command.HeroPrimaryButtonText,
                command.LoginPageImageUrl,
                command.RegisterPageImageUrl);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
