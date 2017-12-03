using System;
using System.Text.RegularExpressions;
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

        public HttpResponse CreateProduct(Product product)
        {
            try
            {
                ValidateName(product.Name);
                ValidateManufacturer(product.Manufacturer);
                if (product.ImporterEmail != null)
                {
                    ValidateEmail(product.ImporterEmail);
                }

                _repository.Add(product);

                _repository.Commit();

                return Response.Ok();
            }
            catch (BusinessException ex)
            {
                return Response.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        public HttpResponse GetProduct(int productId)
        {
            try
            {
                var product = _repository.Find(productId);
                if (product == null)
                    throw new BusinessException($"Product with id {productId} was not found");

                return Response.Ok(product);
            }
            catch (BusinessException ex)
            {
                return Response.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        public HttpResponse Order(int productId, uint quantity)
        {
            try
            {
                var product = _repository.Find(productId);
                if (product == null)
                    throw new BusinessException($"Product with id {productId} was not found");

                if (quantity > (uint)Constants.MaxQuantityInOrder)
                    throw new BusinessException("The order is too large");

                if (product.Quantity < quantity)
                {
                    uint excess = quantity - product.Quantity;
                    uint orderedQuantity = _supplier.Order(productId, product.Manufacturer, excess);
                    if (product.Quantity + orderedQuantity < quantity)
                    {
                        throw new BusinessException("The product is out of stock");
                    }

                    product.Quantity += orderedQuantity;
                }

                product.Quantity -= quantity;

                _repository.Commit();

                return Response.Ok();
            }
            catch (BusinessException ex)
            {
                return Response.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Response.InternalError(ex.Message);
            }
        }

        private void ValidateName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new BusinessException("Product name must not be empty");

            if (productName.Length > 100)
                throw new BusinessException("Product name is too long");
        }

        private void ValidateManufacturer(string manufacturerName)
        {
            if (string.IsNullOrWhiteSpace(manufacturerName))
                throw new BusinessException("Manufacturer name must not be empty");

            if (manufacturerName.Length > 256)
                throw new BusinessException("Manufacturer name is too long");
        }

        private void ValidateEmail(string email)
        {
            if (!Regex.IsMatch(email, @"^(.+)@(.+)$"))
                throw new BusinessException(email + " is invalid");
        }

    }
}