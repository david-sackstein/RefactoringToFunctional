namespace SuperMarket.Service
{
    public interface ISupplierService
    {
        uint Order(int productId, string Manufacturer, uint quantity);
    }
}