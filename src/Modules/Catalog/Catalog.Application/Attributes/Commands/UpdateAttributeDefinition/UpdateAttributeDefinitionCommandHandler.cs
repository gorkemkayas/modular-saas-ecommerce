using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Attributes.Commands.UpdateAttributeDefinition
{
    public sealed class UpdateAttributeDefinitionCommandHandler : IRequestHandler<UpdateAttributeDefinitionCommand>
    {
        private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAttributeDefinitionCommandHandler(
            IAttributeDefinitionRepository attributeDefinitionRepository,
            IUnitOfWork unitOfWork)
        {
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateAttributeDefinitionCommand command, CancellationToken cancellationToken)
        {
            var attributeDefinition = await _attributeDefinitionRepository.GetByIdAsync(
                command.StoreId,
                command.AttributeDefinitionId,
                cancellationToken);

            if (attributeDefinition is null)
                throw new AttributeDefinitionNotFoundException(command.AttributeDefinitionId);

            var code = AttributeCode.Create(command.Code);

            if (await _attributeDefinitionRepository.ExistsByCodeAsync(
                    command.StoreId,
                    code,
                    command.AttributeDefinitionId,
                    cancellationToken))
            {
                throw new DuplicateAttributeCodeException(code.Value);
            }

            attributeDefinition.Rename(command.Name);
            attributeDefinition.ChangeCode(code);
            attributeDefinition.ChangeDataType(command.DataType);
            attributeDefinition.SetRequired(command.IsRequired);
            attributeDefinition.SetFilterable(command.IsFilterable);
            attributeDefinition.SetVariantDefining(command.IsVariantDefining);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
