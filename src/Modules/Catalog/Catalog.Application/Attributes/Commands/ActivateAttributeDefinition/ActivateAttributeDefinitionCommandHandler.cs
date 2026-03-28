using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Attributes.Commands.ActivateAttributeDefinition
{
    public sealed class ActivateAttributeDefinitionCommandHandler : IRequestHandler<ActivateAttributeDefinitionCommand>
    {
        private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateAttributeDefinitionCommandHandler(
            IAttributeDefinitionRepository attributeDefinitionRepository,
            IUnitOfWork unitOfWork)
        {
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ActivateAttributeDefinitionCommand command, CancellationToken cancellationToken)
        {
            var attributeDefinition = await _attributeDefinitionRepository.GetByIdAsync(
                command.StoreId,
                command.AttributeDefinitionId,
                cancellationToken);

            if (attributeDefinition is null)
                throw new AttributeDefinitionNotFoundException(command.AttributeDefinitionId);

            attributeDefinition.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
