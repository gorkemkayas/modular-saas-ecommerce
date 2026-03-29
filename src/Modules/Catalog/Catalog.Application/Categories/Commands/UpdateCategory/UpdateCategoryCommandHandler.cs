using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Categories.Commands.UpdateCategory
{
    public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(command.StoreId, command.CategoryId, cancellationToken);
            if (category is null)
                throw new CategoryNotFoundException(command.CategoryId);

            var slug = Slug.Create(command.Slug);

            if (await _categoryRepository.ExistsBySlugAsync(
                    command.StoreId,
                    slug,
                    command.CategoryId,
                    cancellationToken))
            {
                throw new DuplicateCategorySlugException(slug.Value);
            }

            category.Rename(command.Name);
            category.ChangeSlug(slug);
            category.ChangeDescription(command.Description);
            category.SetSortOrder(command.SortOrder);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
