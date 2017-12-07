using System;
using FunctionalExtensions;
using SuperMarket.Entities;

namespace SuperMarket.Service
{
    public class ProductService
    {
        private readonly IProductRepository _repository;
        private readonly ISupplierService _supplier;

        public ProductService(IProductRepository repository, ISupplierService supplier)
        {
            _repository = repository;
            _supplier = supplier;
        }

        public HttpResponse CreateProduct(ProductDefinition definition)
        {
            Result<ProductName> productName = ProductName.Create(definition.Name);
            Result<ManufacturerName> manufacturer = ManufacturerName.Create(definition.Manufacturer);
            Result<Maybe<Email>> emailOrNothing = GetImporterEmail(definition);

            return Result.Combine(productName, manufacturer, emailOrNothing)
                .OnSuccess(() => new Product
                {
                    ProductId = definition.ProductId,
                    Category = definition.Category,
                    Name = productName.Value,
                    Manufacturer = manufacturer.Value,
                    ImporterEmail = emailOrNothing.Value,
                    Quantity = definition.Quantity
                })
                .OnSuccess(p => _repository.Add(p))
                .OnBoth(r => r.IsSuccess ? Commit() : Response.BadRequest(r.Error));
        }

        private static Result<Maybe<Email>> GetImporterEmail(ProductDefinition definition)
        {
            if (definition.ImporterEmail == null)
            {
                return Result.Ok(new Maybe<Email>());
            }
            return Email.Create(definition.ImporterEmail)
                .Map(x => (Maybe<Email>)x);
        }

        public HttpResponse GetProduct(int productId)
        {
            return _repository.Find(productId)
                .ToResult($"Product with id {productId} was not found")
                .OnSuccess(product =>
                {
                    Maybe<Email> importerEmail = product.ImporterEmail;
                    return new ProductDefinition
                    {
                        ProductId = product.ProductId,
                        Category = product.Category,
                        Name = product.Name.Value,
                        Manufacturer = product.Manufacturer.Value,
                        ImporterEmail = importerEmail.HasValue ? importerEmail.Value.Value : null,
                        Quantity = product.Quantity
                    };
                })
                .OnBoth(t => t.IsSuccess ? Response.Ok(t.Value) : Response.BadRequest(t.Error));
        }

        public HttpResponse Order(int productId, uint quantity)
        {
            var productOrNone = _repository.Find(productId);
            if (productOrNone.HasNoValue)
                return Response.BadRequest($"Product with id {productId} was not found");

            var product = productOrNone.Value;

            if (quantity > (uint)Constants.MaxQuantityInOrder)
                return Response.BadRequest("The order is too large");

            if (product.Quantity < quantity)
            {
                uint excess = quantity - product.Quantity;
                Result<uint> orderedQuantity = OrderFromSupplier(productId, product, excess);
                if (orderedQuantity.IsFailure)
                    return Response.InternalError(orderedQuantity.Error);

                if (product.Quantity + orderedQuantity.Value < quantity)
                {
                    return Response.BadRequest("The product is out of stock");
                }

                product.Quantity += orderedQuantity.Value;
            }

            product.Quantity -= quantity;

            return Commit();
        }

        private HttpResponse Commit()
        {
            try
            {
                _repository.Commit();
                return Response.Ok();
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        private Result<uint> OrderFromSupplier(int productId, Product product, uint excess)
        {
            try
            {
                uint ordered = _supplier.Order(productId, product.Manufacturer, excess);
                return Result.Ok(ordered);
            }
            catch (Exception e)
            {
                return Result.Fail<uint>(e.Message);
            }
        }
    }
}