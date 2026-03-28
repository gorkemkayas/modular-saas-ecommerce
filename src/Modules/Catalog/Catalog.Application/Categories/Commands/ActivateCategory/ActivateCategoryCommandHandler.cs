using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Commands.ActivateCategory
{
    public sealed class ActivateCategoryCommandHandler : IRequestHandler<ActivateCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ActivateCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(command.StoreId, command.CategoryId, cancellationToken);
            if (category is null)
                throw new CategoryNotFoundException(command.CategoryId);

            category.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
