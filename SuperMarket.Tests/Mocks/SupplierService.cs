using System;

namespace SuperMarket.Service
{
    public class SupplierService : ISupplierService
    {
        public uint Available { get; set; }
        public bool Fail { get; set; }

        public uint Order(int productId, string Manufacturer, uint quantity)
        {
            if (Fail)
            {
                throw new InvalidOperationException("Not enough stock");
            }

            if (Available >= quantity)
            {
                Available -= quantity;
                return quantity;
            }

            Available = 0;
            return Available;
        }
    }
}