using WeeklyProject03_AssetTracking.Application.Interfaces;
using WeeklyProject03_AssetTracking.Domain.Entities;

namespace WeeklyProject03_AssetTracking.Infrastruture.Persistence
{
    public class InMemoryAssetRepository : IAssetRepository
    {
        private readonly List<Asset> _assets = new();

        public void Add(Asset asset)
        {
            _assets.Add(asset);
        }

        public List<Asset> GetAll()
        {
            return _assets;
        }
    }
}
