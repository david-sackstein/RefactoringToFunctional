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
            if (productName.IsFailure)
                return Response.BadRequest(productName.Error);

            Result<ManufacturerName> manufacturer = ManufacturerName.Create(definition.Manufacturer);
            if (manufacturer.IsFailure)
                return Response.BadRequest(manufacturer.Error);

            Result<Maybe<Email>> emailOrNothing = Result.Ok(new Maybe<Email>());

            if (definition.ImporterEmail != null)
            {
                Result<Email> email = Email.Create(definition.ImporterEmail);
                if (email.IsFailure)
                    emailOrNothing = Result.Fail<Maybe<Email>>(email.Error);
                else
                    emailOrNothing = Result.Ok<Maybe<Email>>(email.Value);
            }

            if (emailOrNothing.IsFailure)
                return Response.BadRequest(emailOrNothing.Error);

            Product product = new Product
            {
                ProductId = definition.ProductId,
                Category = definition.Category,
                Name = productName.Value,
                Manufacturer = manufacturer.Value,
                ImporterEmail = emailOrNothing.Value,
                Quantity = definition.Quantity
            };

            _repository.Add(product);

            return Commit();
        }

        public HttpResponse GetProduct(int productId)
        {
            Maybe<Product> productOrNone = _repository.Find(productId);
            if (productOrNone.HasNoValue)
                return Response.BadRequest($"Product with id {productId} was not found");

            var product = productOrNone.Value;

            Maybe<Email> importerMail = product.ImporterEmail;

            ProductDefinition definition = new ProductDefinition
            {
                ProductId = product.ProductId,
                Category = product.Category,
                Name = product.Name.Value,
                Manufacturer = product.Manufacturer.Value,
                ImporterEmail = importerMail.HasValue ? importerMail.Value.Value : null,
                Quantity = product.Quantity
            };

            return Response.Ok(definition);
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