using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Commands.ChangeCategoryParent
{
    public sealed class ChangeCategoryParentCommandHandler : IRequestHandler<ChangeCategoryParentCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeCategoryParentCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ChangeCategoryParentCommand command, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(command.StoreId, command.CategoryId, cancellationToken);
            if (category is null)
                throw new CategoryNotFoundException(command.CategoryId);

            if (command.ParentCategoryId.HasValue)
            {
                if (!await _categoryRepository.ExistsByIdAsync(command.StoreId, command.ParentCategoryId.Value, cancellationToken))
                    throw new CategoryNotFoundException(command.ParentCategoryId.Value);

                if (await _categoryRepository.IsDescendantOfAsync(
                        command.StoreId,
                        command.ParentCategoryId.Value,
                        command.CategoryId,
                        cancellationToken))
                {
                    throw new CatalogValidationException("A category cannot be moved under one of its descendants.");
                }
            }

            category.ChangeParent(command.ParentCategoryId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
