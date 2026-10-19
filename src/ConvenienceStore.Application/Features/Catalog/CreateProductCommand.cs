using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Catalog;
using ConvenienceStore.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Catalog;

public record ProductBarcodeInput(Guid UnitOfMeasureId, string BarcodeValue, bool IsPrimary);
public record ProductUnitConversionInput(Guid UnitOfMeasureId, decimal ConversionFactor, decimal SalePrice);
public record CreateProductCommand(
    string Sku,
    string Name,
    Guid CategoryId,
    Guid? BrandId,
    Guid BaseUnitId,
    decimal StandardPrice,
    decimal InitialCostPrice,
    VatRate VatRate,
    bool IsWeightBased,
    string? PluCode,
    List<ProductBarcodeInput> Barcodes,
    List<ProductUnitConversionInput> UnitConversions
) : IRequest<Result<Guid>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StandardPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialCostPrice).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IAppDbContext _dbContext;

    public CreateProductCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        if (await _dbContext.Products.AnyAsync(p => p.Sku == normalizedSku, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Product.DuplicateSku", $"Mã SKU '{normalizedSku}' đã tồn tại."));
        }

        var product = Product.Create(
            normalizedSku,
            request.Name,
            request.CategoryId,
            request.BrandId,
            request.BaseUnitId,
            request.StandardPrice,
            request.InitialCostPrice,
            request.VatRate,
            request.IsWeightBased,
            request.PluCode
        );

        foreach (var b in request.Barcodes)
        {
            product.AddBarcode(b.UnitOfMeasureId, b.BarcodeValue, b.IsPrimary);
        }

        foreach (var uc in request.UnitConversions)
        {
            product.AddUnitConversion(uc.UnitOfMeasureId, uc.ConversionFactor, uc.SalePrice);
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
