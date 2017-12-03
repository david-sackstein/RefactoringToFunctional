using System;

namespace SuperMarket.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public Category Category { get; set; }
        public ProductName Name { get; set; }
        public ManufacturerName Manufacturer { get; set; } 
        public Email ImporterEmail { get; set; } 
        public uint Quantity { get; set; }
    }
}