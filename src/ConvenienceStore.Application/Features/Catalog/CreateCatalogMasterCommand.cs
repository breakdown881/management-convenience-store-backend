using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Catalog;
using MediatR;

namespace ConvenienceStore.Application.Features.Catalog;

public record CreateCategoryCommand(string Code, string Name, Guid? ParentId = null) : IRequest<Result<Guid>>;
public record CreateBrandCommand(string Code, string Name, string? CountryOfOrigin = null) : IRequest<Result<Guid>>;
public record CreateUnitOfMeasureCommand(string Code, string Name) : IRequest<Result<Guid>>;

public class CatalogMasterCommandHandlers :
    IRequestHandler<CreateCategoryCommand, Result<Guid>>,
    IRequestHandler<CreateBrandCommand, Result<Guid>>,
    IRequestHandler<CreateUnitOfMeasureCommand, Result<Guid>>
{
    private readonly IAppDbContext _dbContext;

    public CatalogMasterCommandHandlers(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Code, request.Name, request.ParentId);
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(category.Id);
    }

    public async Task<Result<Guid>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = Brand.Create(request.Code, request.Name, request.CountryOfOrigin);
        _dbContext.Brands.Add(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(brand.Id);
    }

    public async Task<Result<Guid>> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var uom = UnitOfMeasure.Create(request.Code, request.Name);
        _dbContext.UnitOfMeasure.Add(uom);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(uom.Id);
    }
}
