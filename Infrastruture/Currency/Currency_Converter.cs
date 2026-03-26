namespace WeeklyProject03_AssetTracking.Infrastruture.Currency
{
    public class CurrencyConverter
    {
        private readonly Dictionary<string, decimal> _rates = new()
        {
            { "USA", 1.0m },
            { "Sweden", 10.5m },
            { "EU", 0.9m }
        };

        public decimal Convert(decimal amount, string office)
        {
            return amount * (_rates.ContainsKey(office) ? _rates[office] : 1);
        }
    }
}
