namespace SuperMarket.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; } 
        public string ImporterEmail { get; set; } 
        public uint Quantity { get; set; }
    }
}