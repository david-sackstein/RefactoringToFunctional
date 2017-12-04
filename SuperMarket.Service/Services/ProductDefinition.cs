using NullGuard;
using SuperMarket.Entities;

namespace SuperMarket.Service
{
    public class ProductDefinition
    {
        public int ProductId { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        [AllowNull]
        public string ImporterEmail { get; set; }
        public uint Quantity { get; set; }
    }
}