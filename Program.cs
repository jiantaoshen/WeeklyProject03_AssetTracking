using System.Globalization;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

List<Asset> assets = new List<Asset>();
var rates = await LoadEcbRatesAsync();

//Add items until user enter "q" and print the list
while (true)
{

    ColoredString("To enter a new product - follow the steps | To quit - enter: \"Q\" ", ConsoleColor.Yellow);
    Console.Write("Enter an Office: ");
    string inputOffice = Console.ReadLine() ?? "Unknown";

    if (inputOffice.Trim().ToLower() == "q") break;

    int typeNumber;

    while (true)
    {
        Console.Write("Enter 1 or 2 (1:Computer 2:Phone): ");
        string inputType = Console.ReadLine();

        if (int.TryParse(inputType, out typeNumber) && (typeNumber == 1 || typeNumber == 2))
            break;

        Console.WriteLine("Invalid input. Try again.");
    }

    Console.Write("Enter a Brand: ");
    string inputBrand = Console.ReadLine() ?? "Unknown";

    Console.Write("Enter a Model Name: ");
    string inputModelName= Console.ReadLine() ?? "Unknown";

    DateTime validDate;

    while (true)
    {
        Console.Write("Enter a Purchase date (yyyy-mm-dd): ");
        string inputDate = Console.ReadLine() ?? "Unknown";

        string format = "yyyy-MM-dd";

        bool isValid = DateTime.TryParseExact(inputDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out validDate);

        if (isValid) break;
        else Console.WriteLine("Invalid date format. Please try again. ");
    }

    while (true)
    {
        Console.Write("Enter Local Currency (EUR, SEK, USD): ");
        string? inputCurrency = Console.ReadLine();

        if (inputCurrency != null) inputCurrency = inputCurrency?.Trim().ToUpper() ?? "USD";
        else
        {
            Console.WriteLine("Set to Default currency: USD");
            inputCurrency = "USD";
        }

        Console.Write("Enter Local price: ");
        string? inputPrice = Console.ReadLine();

        //Error handling of the price
        if (decimal.TryParse(inputPrice, out decimal roundPrice))
        {
            roundPrice = Math.Round(roundPrice, 2); //Round to 2 decimals
            if (typeNumber == 1) assets.Add(new Computer(inputBrand, inputModelName, inputOffice, validDate, roundPrice, inputCurrency));
            if (typeNumber == 2) assets.Add(new Phone(inputBrand, inputModelName, inputOffice, validDate, roundPrice, inputCurrency));
            break;
        }
        else Console.WriteLine("Invalid input of price. Please try again. ");
    }
}

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

    decimal priceInDollar = ConvertCurrency(asset.priceInLocal, asset.currencyLocal, "USD");
    string strPriceInDollar = $"{priceInDollar:F2}".PadRight(20);
    Console.WriteLine($"{asset.office.PadRight(15)}{asset.type.PadRight(15)}{asset.brand.PadRight(20)}{asset.modelName.PadRight(20)}{strPriceInDollar}{asset.priceInLocal.ToString().PadRight(20)}{asset.purchaseDate.ToShortDateString().PadRight(20)}");
}

Console.ResetColor();
Console.ReadLine();

//Functions
void ColoredString(string text, ConsoleColor consoleColor)
{
    Console.ForegroundColor = consoleColor;
    Console.WriteLine(text);
    Console.ResetColor();
}

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
    public decimal priceInLocal { get; set; }
    public string currencyLocal { get; set; }

    public DateTime endOfLife => purchaseDate.AddYears(3);

    public Asset(string brand, string modelName, string office, DateTime purchaseDate, decimal priceInLocal, string currencyLocal)
    {
        this.brand = brand;
        this.modelName = modelName;
        this.office = office;
        this.purchaseDate = purchaseDate;
        this.priceInLocal = priceInLocal;
        this.currencyLocal = currencyLocal;
    }
}

// Computer class
class Computer : Asset
{
   public override string type => "Computer";
   public Computer(string brand, string modelName, string office, DateTime purchaseDate, decimal priceInLocal, string currencyLocal) : base(brand, modelName, office, purchaseDate, priceInLocal, currencyLocal)
    {
    }
}

//Phone class
class Phone : Asset
{
    public override string type => "Phone";
    public Phone(string brand, string modelName, string office, DateTime purchaseDate, decimal priceInLocal, string currencyLocal) : base(brand, modelName, office, purchaseDate, priceInLocal, currencyLocal)
    {
    }
}