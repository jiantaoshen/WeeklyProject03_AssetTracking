namespace WeeklyProject03_AssetTracking.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
