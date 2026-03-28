using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Attributes.Commands.DeactivateAttributeDefinition
{
    public sealed class DeactivateAttributeDefinitionCommandHandler : IRequestHandler<DeactivateAttributeDefinitionCommand>
    {
        private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateAttributeDefinitionCommandHandler(
            IAttributeDefinitionRepository attributeDefinitionRepository,
            IUnitOfWork unitOfWork)
        {
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeactivateAttributeDefinitionCommand command, CancellationToken cancellationToken)
        {
            var attributeDefinition = await _attributeDefinitionRepository.GetByIdAsync(
                command.StoreId,
                command.AttributeDefinitionId,
                cancellationToken);

            if (attributeDefinition is null)
                throw new AttributeDefinitionNotFoundException(command.AttributeDefinitionId);

            attributeDefinition.Deactivate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
