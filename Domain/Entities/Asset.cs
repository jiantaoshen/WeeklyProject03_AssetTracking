namespace WeeklyProject03_AssetTracking.Domain.Entities
{
    public abstract class Asset
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Price { get; set; }
        public string Office { get; set; }
        public string Currency { get; set; }

        public abstract string Type { get; }

        public DateTime EndOfLife => PurchaseDate.AddYears(3);
    }

    public class Computer : Asset
    {
        public override string Type => "Computer";
    }

    public class Phone : Asset
    {
        public override string Type => "Phone";
    }
}
