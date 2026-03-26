using WeeklyProject03_AssetTracking.Application.Interfaces;
using WeeklyProject03_AssetTracking.Domain.Services;

public class ListAssets
{
    private readonly IAssetRepository _repo;
    private readonly ICurrencyService _currencyService;

    public ListAssets(IAssetRepository repo, ICurrencyService currencyService)
    {
        _repo = repo;
        _currencyService = currencyService;
    }

    public async Task<List<string>> ExecuteAsync()
    {
        var assets = _repo.GetAll();

        var sorted = assets
            .OrderBy(a => a.Office)
            .ThenBy(a => a.Type)
            .ThenBy(a => a.PurchaseDate)
            .ToList();

        var result = new List<string>();

        foreach (var a in sorted)
        {
            decimal convertedPrice = await _currencyService.ConvertAsync(
                a.Price,
                a.Currency,
                "USD"
            );

            string color = AssetService.GetColor(a);

            string line =
                $"{a.Office,-15}" +
                $"{a.Type,-15}" +
                $"{a.Brand,-15}" +
                $"{a.Model,-15}" +
                $"{a.PurchaseDate.ToString("yyyy-MM-dd"),-15}" +
                $"{a.Price,-15:F2}" +
                $"{a.Currency,-15}" +
                $"{convertedPrice,-15:F2}";

            // Add color indicator
            if (color == "RED")
                line = "[RED] " + line;
            else if (color == "YELLOW")
                line = "[YELLOW] " + line;

            result.Add(line);
        }

        return result;
    }
}