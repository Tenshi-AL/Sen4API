using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Helpers;

namespace Persistence.ModelConfigurations;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        var operationHelperInfo = OperationHelper.GetOperations();
        var operations = new List<Operation>();
        foreach (var operationHelperItem in operationHelperInfo)
        {
            foreach (var action in operationHelperItem.Actions)
            {
                operations.Add(new Operation()
                {
                    Id = Guid.NewGuid(),
                    Controller = operationHelperItem.Controller,
                    Action = action.Name,
                    Description = action.Description,
                });
            }
        }

        builder.HasData(operations);
    }
}