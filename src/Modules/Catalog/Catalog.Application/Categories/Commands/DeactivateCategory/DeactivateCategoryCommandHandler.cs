using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Commands.DeactivateCategory
{
    public sealed class DeactivateCategoryCommandHandler : IRequestHandler<DeactivateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeactivateCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(command.StoreId, command.CategoryId, cancellationToken);
            if (category is null)
                throw new CategoryNotFoundException(command.CategoryId);

            category.Deactivate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
