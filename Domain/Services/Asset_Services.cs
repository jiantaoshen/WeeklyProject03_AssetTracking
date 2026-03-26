using WeeklyProject03_AssetTracking.Domain.Entities;

namespace WeeklyProject03_AssetTracking.Domain.Services
{
    public static class AssetService
    {
        public static string GetColor(Asset asset)
        {
            if (DateTime.Now >= asset.EndOfLife.AddMonths(-3))
                return "RED";
            if (DateTime.Now >= asset.EndOfLife.AddMonths(-6))
                return "YELLOW";
            return "NORMAL";
        }
    }
}
