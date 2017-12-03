using SuperMarket.Entities;

namespace SuperMarket.Service
{
    public interface IProductRepository
    {
        void Add(Product product);
        Product Find(int productId);
        void Commit();
    }
}