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
            string nameError = ValidateName(product.Name);
            if (nameError != string.Empty)
                return Response.BadRequest(nameError);

            string manufacturerError = ValidateManufacturer(product.Manufacturer);
            if (manufacturerError != string.Empty)
                return Response.BadRequest(manufacturerError);

            if (product.ImporterEmail != null)
            {
                string emailError = ValidateEmail(product.ImporterEmail);
                if (emailError != string.Empty)
                    return Response.BadRequest(emailError);
            }

            _repository.Add(product);

            return Commit();
        }

        public HttpResponse GetProduct(int productId)
        {
            var product = _repository.Find(productId);
            if (product == null)
                return Response.BadRequest($"Product with id {productId} was not found");

            return Response.Ok(product);
        }

        public HttpResponse Order(int productId, uint quantity)
        {
            var product = _repository.Find(productId);
            if (product == null)
                return Response.BadRequest($"Product with id {productId} was not found");

            if (quantity > (uint)Constants.MaxQuantityInOrder)
                return Response.BadRequest("The order is too large");

            if (product.Quantity < quantity)
            {
                uint excess = quantity - product.Quantity;
                string supplierError = OrderFromSupplier(productId, product, excess, out uint orderedQuantity);
                if (supplierError != String.Empty)
                    return Response.InternalError(supplierError);

                if (product.Quantity + orderedQuantity < quantity)
                {
                    return Response.BadRequest("The product is out of stock");
                }

                product.Quantity += orderedQuantity;
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

        private string OrderFromSupplier(int productId, Product product, uint excess, out uint ordered)
        {
            try
            {
                ordered = _supplier.Order(productId, product.Manufacturer, excess);
                return String.Empty;
            }
            catch (Exception e)
            {
                ordered = 0;
                return e.Message;
            }
        }

        private string ValidateName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return "Product name must not be empty";

            if (productName.Length > 100)
                return "Product name is too long";

            return string.Empty;
        }

        private string ValidateManufacturer(string manufacturerName)
        {
            if (string.IsNullOrWhiteSpace(manufacturerName))
                return "Manufacturer name must not be empty";

            if (manufacturerName.Length > 256)
                return "Manufacturer name is too long";

            return string.Empty;
        }

        private string ValidateEmail(string email)
        {
            if (!Regex.IsMatch(email, @"^(.+)@(.+)$"))
                return email + " is invalid";

            return String.Empty;
        }

    }
}