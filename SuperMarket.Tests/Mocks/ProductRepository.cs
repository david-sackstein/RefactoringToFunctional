using System.Collections.Generic;
using System.Data;
using SuperMarket.Entities;

namespace SuperMarket.Service
{
    public class ProductRepository : IProductRepository
    {
        private static readonly Dictionary<int, Product> _persistent;
        private readonly Dictionary<int, Product> _products;

        private bool ThrowOnCommit { get; set; }

        static ProductRepository()
        {
            _persistent = new Dictionary<int, Product>();
        }

        public ProductRepository()
        {
            _products = new Dictionary<int, Product>();
        }

        public static void DeleteDatabase()
        {
            _persistent.Clear();
        }

        public void Add(Product product)
        {
            if (_products.ContainsKey(product.ProductId))
            {
                _products.Remove(product.ProductId);
            }
            _products.Add(product.ProductId, product);
        }

        public Product Find(int productId)
        {
            if (_persistent.ContainsKey(productId))
            {
                return _persistent[productId];
            }
            return null;
        }

        public void Commit()
        {
            if (ThrowOnCommit)
            {
                throw new DataException("Failed to store products");
            }

            foreach (var kv in _products)
            {
                if (_persistent.ContainsKey(kv.Key))
                {
                    _persistent.Remove(kv.Key);
                }
                _persistent.Add(kv.Key, kv.Value);
            }

            _products.Clear();
        }
    }
}