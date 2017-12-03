using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SuperMarket.Entities;
using SuperMarket.Service;

namespace SuperMarket.Tests
{
    [TestClass]
    public class UnitTests
    {
        private ProductService _service;
        private ProductRepository _repository;
        private SupplierService _supplier;
        private Product _product;

        [TestInitialize]
        public void SetUp()
        {
            ProductRepository.DeleteDatabase();

            _repository = new ProductRepository();
            _supplier = new SupplierService();
            _service = new ProductService(_repository, _supplier);

            _product = new Product
            {
                ProductId = 1,
                Category = Category.Food,
                Name = "Oranges",
                Manufacturer = "Jaffa",
                ImporterEmail = "jaffa@gmail.com",
                Quantity = 1000
            };

            _service.CreateProduct(_product);
        }

        [TestMethod]
        public void TestCreateInvalidProductNameFails()
        {
            var product = CreateProduct(2);
            product.Name = new String('c', 1000);

            HttpResponse response = _service.CreateProduct(product);
            Assert.AreEqual(ResponseCode.BadRequest, response.ResponseCode);
        }

        [TestMethod]
        public void TestCreateInvalidManufacturerNameFails()
        {
            var product = CreateProduct(2);
            product.Manufacturer = new String('c', 1000);

            HttpResponse response = _service.CreateProduct(product);
            Assert.AreEqual(ResponseCode.BadRequest, response.ResponseCode);
        }

        [TestMethod]
        public void TestCreateWithoutImporterEmailSucceeds()
        {
            var product = CreateProduct(2);
            product.ImporterEmail = null;

            HttpResponse response = _service.CreateProduct(product);
            Assert.AreEqual(ResponseCode.Ok, response.ResponseCode);
        }

        [TestMethod]
        public void TestCreateWithInvalidImporterEmailFails()
        {
            var product = CreateProduct(2);
            product.ImporterEmail = "invalidemail";

            HttpResponse response = _service.CreateProduct(product);
            Assert.AreEqual(ResponseCode.BadRequest, response.ResponseCode);
        }

        [TestMethod]
        public void TestInvalidProductIdFails()
        {
            HttpResponse response = _service.GetProduct(2);
            Assert.AreEqual(ResponseCode.BadRequest, response.ResponseCode);
        }

        [TestMethod]
        public void TestValidProductIdSucceeds()
        {
            var productId = _product.ProductId;

            HttpResponse response = _service.GetProduct(productId);

            Assert.AreEqual(response.ResponseCode, ResponseCode.Ok);
            var product = JsonConvert.DeserializeObject<Product>(response.JsonValue);
            Assert.AreEqual(productId, product.ProductId);
        }

        [TestMethod]
        public void TestSmallOrderSucceeds()
        {
            var productId = _product.ProductId;
            uint originalQuantity = _product.Quantity;
            uint orderQuantity = originalQuantity / 2;

            HttpResponse response = _service.Order(productId, orderQuantity);
            Assert.AreEqual(ResponseCode.Ok, response.ResponseCode);

            response = _service.GetProduct(productId);
            var product = JsonConvert.DeserializeObject<Product>(response.JsonValue);

            Assert.AreEqual(originalQuantity - orderQuantity, product.Quantity);
        }

        [TestMethod]
        public void TestLargeOrderWhenSupplyAvailableSucceeds()
        {
            var productId = _product.ProductId;
            uint originalQuantity = _product.Quantity;
            uint excessQuantity = 100;

            uint orderQuantity = originalQuantity + excessQuantity;
            _supplier.Available = excessQuantity;

            HttpResponse response = _service.Order(productId, orderQuantity);
            Assert.AreEqual(ResponseCode.Ok, response.ResponseCode);

            response = _service.GetProduct(productId);
            var product = JsonConvert.DeserializeObject<Product>(response.JsonValue);

            Assert.IsTrue(product.Quantity == 0);
        }

        [TestMethod]
        public void TestLargeOrderWhenSupplierRefusesFails()
        {
            var productId = _product.ProductId;
            uint originalQuantity = _product.Quantity;
            uint excessQuantity = (uint)Constants.MaxQuantityInOrder + 1;

            uint orderQuantity = originalQuantity + excessQuantity;
            _supplier.Available = excessQuantity;

            HttpResponse response = _service.Order(productId, orderQuantity);
            Assert.AreEqual(ResponseCode.BadRequest, response.ResponseCode);

            response = _service.GetProduct(productId);
            var product = JsonConvert.DeserializeObject<Product>(response.JsonValue);

            // quantity should not have changed
            Assert.IsTrue(product.Quantity == _product.Quantity);
        }

        [TestMethod]
        public void TestLargeOrderWhenSupplierThrowsFails()
        {
            var productId = _product.ProductId;
            uint originalQuantity = _product.Quantity;
            uint excessQuantity = 100;

            uint orderQuantity = originalQuantity + excessQuantity;
            _supplier.Available = excessQuantity;
            _supplier.Fail = true;

            HttpResponse response = _service.Order(productId, orderQuantity);
            Assert.AreEqual(ResponseCode.InternalError, response.ResponseCode);

            response = _service.GetProduct(productId);
            var product = JsonConvert.DeserializeObject<Product>(response.JsonValue);

            // quantity should not have changed
            Assert.IsTrue(product.Quantity == _product.Quantity);
        }

        private static Product CreateProduct(int productId)
        {
            return new Product
            {
                ProductId = productId,
                Category = Category.Food,
                Name = "Apples",
                Manufacturer = "Jaffa",
                ImporterEmail = "jaffa@gmail.com",
                Quantity = 1000
            };
        }
    }
}
