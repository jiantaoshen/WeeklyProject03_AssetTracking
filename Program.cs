using System.Globalization;
using System.Xml.Linq;

List<Asset> assets = new List<Asset>();
var rates = await LoadEcbRatesAsync();

//Example assets from the documentation with modification
assets.Add(new Phone("iPhone", "8", "Spain", DateTime.Now.AddMonths(-36 + 2), 1999m, "EUR"));
assets.Add(new Computer("HP", "Elitebook", "Spain", DateTime.Now.AddMonths(-36 + 14), 3999m, "EUR"));
assets.Add(new Phone("iPhone", "11", "Spain", DateTime.Now.AddMonths(-36 + 9), 3999m, "EUR"));
assets.Add(new Phone("iPhone", "X", "Sweden", DateTime.Now.AddMonths(-36 + 8), 2999m, "SEK"));
assets.Add(new Phone("Motorola", "Razr", "Sweden", DateTime.Now.AddMonths(-36 + 5), 999m, "SEK"));
assets.Add(new Computer("HP", "Elitebook", "Sweden", DateTime.Now.AddMonths(-36 + 1), 1599m, "SEK"));
assets.Add(new Computer("ASUS", "W234", "USA", DateTime.Now.AddMonths(-36 + 8), 1299m, "USD"));
assets.Add(new Computer("Lenovo", "Yoga 730", "USA", DateTime.Now.AddMonths(-36 + 12), 899m, "USD"));
assets.Add(new Computer("Lenovo", "Yoga 530", "USA", DateTime.Now.AddMonths(-36 + 7), 1999m, "USD"));

// Sorting: first office, then purchase date
var sortedAssets = assets
    .OrderBy(a => a.office)   
    .ThenBy(a => a.purchaseDate)
    .ToList();

Console.WriteLine("Office".PadRight(15) + "Asset".PadRight(15) + "Brand".PadRight(20) + "Model".PadRight(20) + "Price (USD)".PadRight(20) + "Price (Local)".PadRight(20) + "Purchase Date".PadRight(20));
Console.WriteLine(new string('-', 130));

//Print
foreach (var asset in sortedAssets)
{
    if (DateTime.Now >= asset.endOfLife.AddMonths(-6) && DateTime.Now < asset.endOfLife.AddMonths(-3))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

    }
    else if (DateTime.Now >= asset.endOfLife.AddMonths(-3))
    {
        Console.ForegroundColor = ConsoleColor.Red;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.White;
    }

    decimal priceInLocal = ConvertCurrency(asset.priceInDollar, "USD", asset.currencyLocal);
    string strPriceInLocal = $"{priceInLocal:F2}".PadRight(20);
    Console.WriteLine($"{asset.office.PadRight(15)}{asset.type.PadRight(15)}{asset.brand.PadRight(20)}{asset.modelName.PadRight(20)}{asset.priceInDollar.ToString().PadRight(20)}{strPriceInLocal}{asset.purchaseDate.ToShortDateString().PadRight(20)}");
}

Console.ResetColor();
Console.ReadLine();

//Functions
static async Task<Dictionary<string, decimal>> LoadEcbRatesAsync()
{
    string url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

    using HttpClient client = new HttpClient();
    string xml = await client.GetStringAsync(url);

    XDocument doc = XDocument.Parse(xml);
    XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

    var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["EUR"] = 1m // Base currency
    };

    // Navigate to Cube with actual rates
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

decimal ConvertCurrency(decimal amount, string from, string to)
{
    if (!rates.TryGetValue(from, out decimal fromRate))
        throw new Exception($"Currency '{from}' not found");

    if (!rates.TryGetValue(to, out decimal toRate))
        throw new Exception($"Currency '{to}' not found");

    // Convert from “from” to EUR, then to “to”
    decimal inEur = amount / fromRate;
    decimal converted = (inEur * toRate);
    return converted;
}



//Base class
class Asset
{
    public virtual string type => "Asset";

    public string brand { get; set; }
    public string modelName { get; set; }
    public string office { get; set; }
    public DateTime purchaseDate { get; set; }
    public decimal priceInDollar { get; set; }
    public string currencyLocal { get; set; }

    public DateTime endOfLife => purchaseDate.AddYears(3);

    public Asset(string brand, string modelName, string office, DateTime purchaseDate, decimal priceInDollar, string currencyLocal)
    {
        this.brand = brand;
        this.modelName = modelName;
        this.office = office;
        this.purchaseDate = purchaseDate;
        this.priceInDollar = priceInDollar;
        this.currencyLocal = currencyLocal;
    }
}

// Computer class
class Computer : Asset
{
    public override string type => "Computer";
   public Computer(string brand, string modelName, string office, DateTime purchaseDate, decimal priceInDollar, string currencyLocal) : base(brand, modelName, office, purchaseDate, priceInDollar, currencyLocal)
    {
    }
}

//Phone class
class Phone : Asset
{
    public override string type => "Phone";
    public Phone(string brand, string modelName, string office, DateTime purchaseDate, decimal priceInDollar, string currencyLocal) : base(brand, modelName, office, purchaseDate, priceInDollar, currencyLocal)
    {
    }
}
