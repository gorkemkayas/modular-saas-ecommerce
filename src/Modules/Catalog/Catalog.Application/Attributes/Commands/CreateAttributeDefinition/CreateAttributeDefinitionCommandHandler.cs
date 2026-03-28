using Catalog.Application.Abstractions;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Attributes.Commands.CreateAttributeDefinition
{
    public sealed class CreateAttributeDefinitionCommandHandler : IRequestHandler<CreateAttributeDefinitionCommand, Guid>
    {
        private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateAttributeDefinitionCommandHandler> _logger;

        public CreateAttributeDefinitionCommandHandler(
            IAttributeDefinitionRepository attributeDefinitionRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateAttributeDefinitionCommandHandler> logger)
        {
            _attributeDefinitionRepository = attributeDefinitionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateAttributeDefinitionCommand command, CancellationToken cancellationToken)
        {
            if (command.StoreId == Guid.Empty)
                throw new CatalogValidationException("StoreId is required.");

            var code = AttributeCode.Create(command.Code);

            if (await _attributeDefinitionRepository.ExistsByCodeAsync(command.StoreId, code, cancellationToken: cancellationToken))
                throw new DuplicateAttributeCodeException(code.Value);

            var attributeDefinition = AttributeDefinition.Create(
                command.StoreId,
                command.Name,
                code,
                command.DataType,
                command.IsRequired,
                command.IsFilterable,
                command.IsVariantDefining);

            await _attributeDefinitionRepository.AddAsync(attributeDefinition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Catalog attribute definition created | AttributeDefinitionId: {AttributeDefinitionId} | StoreId: {StoreId}",
                attributeDefinition.Id,
                attributeDefinition.StoreId);

            return attributeDefinition.Id;
        }
    }
}
