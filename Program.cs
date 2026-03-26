using System.Globalization;
using WeeklyProject03_AssetTracking.Application.Use_Cases;
using WeeklyProject03_AssetTracking.Domain.Entities;
using WeeklyProject03_AssetTracking.Infrastruture.Persistence;
using static System.Net.Mime.MediaTypeNames;

public static class Program
{
    static async Task Main(string[] args)
    {
        var repo = new InMemoryAssetRepository();
        var currencyService = new CurrencyService();

        var addAsset = new AddAsset(repo);
        var listAssets = new ListAssets(repo, currencyService);

        await Run(addAsset, currencyService);

        //Speed up testing by adding some assets before showing the list
        var testAssets = new List<Asset>
        {
            new Phone { Brand="Motorola", Model="X3", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-32), Price=200, Currency="USD" },
            new Phone { Brand="Motorola", Model="X3", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-31), Price=400, Currency="USD" },
            new Phone { Brand="Motorola", Model="X2", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-26), Price=400, Currency="USD" },

            new Phone { Brand="Samsung", Model="Galaxy 10", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-30), Price=4500, Currency="SEK" },
            new Phone { Brand="Samsung", Model="Galaxy 10", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-29), Price=4500, Currency="SEK" },

            new Phone { Brand="Sony", Model="Xperia 7", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-32), Price=3000, Currency="SEK" },
            new Phone { Brand="Sony", Model="Xperia 7", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-31), Price=3000, Currency="SEK" },

            new Phone { Brand="Siemens", Model="Brick", Office="Germany", PurchaseDate=DateTime.Now.AddMonths(-24), Price=220, Currency="EUR" },

            new Computer { Brand="Dell", Model="Desktop 900", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-38), Price=100, Currency="USD" },
            new Computer { Brand="Dell", Model="Desktop 900", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-37), Price=100, Currency="USD" },

            new Computer { Brand="Lenovo", Model="X100", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-35), Price=300, Currency="USD" },
            new Computer { Brand="Lenovo", Model="X200", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-32), Price=300, Currency="USD" },
            new Computer { Brand="Lenovo", Model="X300", Office="USA", PurchaseDate=DateTime.Now.AddMonths(-27), Price=500, Currency="USD" },

            new Computer { Brand="Dell", Model="Optiplex 100", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-29), Price=1500, Currency="SEK" },
            new Computer { Brand="Dell", Model="Optiplex 200", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-28), Price=1400, Currency="SEK" },
            new Computer { Brand="Dell", Model="Optiplex 300", Office="Sweden", PurchaseDate=DateTime.Now.AddMonths(-27), Price=1300, Currency="SEK" },

            new Computer { Brand="Asus", Model="ROG 600", Office="Germany", PurchaseDate=DateTime.Now.AddMonths(-22), Price=1600, Currency="EUR" },
            new Computer { Brand="Asus", Model="ROG 500", Office="Germany", PurchaseDate=DateTime.Now.AddMonths(-32), Price=1200, Currency="EUR" },
            new Computer { Brand="Asus", Model="ROG 500", Office="Germany", PurchaseDate=DateTime.Now.AddMonths(-33), Price=1200, Currency="EUR" },
            new Computer { Brand="Asus", Model="ROG 500", Office="Germany", PurchaseDate=DateTime.Now.AddMonths(-34), Price=1300, Currency="EUR" }
        };

        foreach (var asset in testAssets)
        {
            addAsset.Execute(asset);
        }

        await ShowList(listAssets);
    }

    public static async Task Run(AddAsset addAsset, CurrencyService currencyService)
    {
        var rates = await currencyService.GetRatesAsync();

        while (true)
        {
            Console.WriteLine("To enter a new product - follow the steps | To quit - enter: \"Q\" ");

            Console.Write("Enter an Office: ");
            string inputOffice = Console.ReadLine() ?? "USA";

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
            string inputModelName = Console.ReadLine() ?? "Unknown";

            DateTime validDate;

            while (true)
            {
                Console.Write("Enter a Purchase date (yyyy-mm-dd): ");
                string inputDate = Console.ReadLine() ?? "";

                bool isValid = DateTime.TryParseExact(
                    inputDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out validDate);

                if (isValid) break;

                Console.WriteLine("Invalid date format. Please try again.");
            }

            decimal price;

            while (true)
            {
                Console.Write($"Enter Price (Local Currency): ");
                string inputPrice = Console.ReadLine();

                if (decimal.TryParse(inputPrice, out price))
                {
                    price = Math.Round(price, 2);
                    break;
                }

                Console.WriteLine("Invalid input of price. Please try again.");
            }

            string inputCurrency;

            while (true)
            {
                Console.Write("Enter Currency (e.g. USD, EUR, SEK): ");
                inputCurrency = Console.ReadLine()?.Trim().ToUpper() ?? "";

                if (rates.ContainsKey(inputCurrency))
                    
                    break;

                Console.WriteLine("Invalid currency. Try again.");
            }

            Asset asset = typeNumber == 1 ? new Computer() : new Phone();

            asset.Brand = inputBrand;
            asset.Model = inputModelName;
            asset.Office = inputOffice;
            asset.PurchaseDate = validDate;
            asset.Price = price;
            asset.Currency = inputCurrency;

            addAsset.Execute(asset);

            Console.WriteLine("Asset added successfully!\n");
        }
    }

    public static async Task ShowList(ListAssets listAssets)
    {
        Console.Clear();

        var lines = await listAssets.ExecuteAsync();

        string header =
            $"{"Office",-15}" +
            $"{"Type",-15}" +
            $"{"Brand",-15}" +
            $"{"Model",-15}" +
            $"{"Purchase Date",-15}" +
            $"{"Price (Local)",-15}" +
            $"{"Currency",-15}" +
            $"{"Price (Dollar)",-15}";

        Console.WriteLine("\n" + header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var line in lines)
        {
            if (line.StartsWith("[RED]"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(line.Replace("[RED] ", ""));
            }
            else if (line.StartsWith("[YELLOW]"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(line.Replace("[YELLOW] ", ""));
            }
            else
            {
                Console.ResetColor();
                Console.WriteLine(line);
            }
        }

        Console.ResetColor();
    }
}