using System.Globalization;
using System.Xml.Linq;
using WeeklyProject03_AssetTracking.Application.Interfaces;

public class CurrencyService : ICurrencyService
{
    public async Task<Dictionary<string, decimal>> GetRatesAsync()
    {
        return await LoadEcbRatesAsync();
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        var rates = await LoadEcbRatesAsync();

        if (!rates.ContainsKey(fromCurrency) || !rates.ContainsKey(toCurrency))
            throw new Exception("Unsupported currency");

        // Convert to EUR first, then to target
        decimal amountInEur = amount / rates[fromCurrency];
        decimal result = amountInEur * rates[toCurrency];

        return result;
    }

    private static async Task<Dictionary<string, decimal>> LoadEcbRatesAsync()
    {
        string url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

        using HttpClient client = new HttpClient();
        string xml = await client.GetStringAsync(url);

        XDocument doc = XDocument.Parse(xml);
        XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

        var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["EUR"] = 1m
        };

        var dailyCube = doc.Root.Element(ns + "Cube")?.Element(ns + "Cube");
        if (dailyCube == null)
            throw new Exception("ECB XML structure changed");

        foreach (var cube in dailyCube.Elements(ns + "Cube"))
        {
            string currency = (string)cube.Attribute("currency");
            string rateStr = (string)cube.Attribute("rate");

            if (!string.IsNullOrEmpty(currency) &&
                decimal.TryParse(rateStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rate))
            {
                rates[currency] = rate;
            }
        }

        return rates;
    }
}