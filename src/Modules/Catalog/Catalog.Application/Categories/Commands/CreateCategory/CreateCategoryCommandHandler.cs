using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Application.Products;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Subscription.Contracts;

namespace Catalog.Application.Categories.Commands.CreateCategory
{
    public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubscriptionModuleApi _subscriptionModuleApi;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        public CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ISubscriptionModuleApi subscriptionModuleApi,
            IUnitOfWork unitOfWork,
            ILogger<CreateCategoryCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _subscriptionModuleApi = subscriptionModuleApi;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            if (command.StoreId == Guid.Empty)
                throw new CatalogValidationException("StoreId is required.");

            var slug = Slug.Create(command.Slug);

            if (await _categoryRepository.ExistsBySlugAsync(command.StoreId, slug, cancellationToken: cancellationToken))
                throw new DuplicateCategorySlugException(slug.Value);

            if (command.ParentCategoryId.HasValue &&
                !await _categoryRepository.ExistsByIdAsync(command.StoreId, command.ParentCategoryId.Value, cancellationToken))
            {
                throw new CategoryNotFoundException(command.ParentCategoryId.Value);
            }

            var currentCategoryCount = await _categoryRepository.CountActiveByStoreIdAsync(
                command.StoreId,
                cancellationToken);

            await CatalogSubscriptionGuard.EnsureCanCreateCategoryAsync(
                command.StoreId,
                currentCategoryCount,
                _subscriptionModuleApi,
                cancellationToken);

            var category = Category.Create(
                command.StoreId,
                command.Name,
                slug,
                command.Description,
                command.ImageUrl,
                command.ParentCategoryId,
                command.SortOrder);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Catalog category created | CategoryId: {CategoryId} | StoreId: {StoreId}",
                category.Id,
                category.StoreId);

            return category.Id;
        }
    }
}
