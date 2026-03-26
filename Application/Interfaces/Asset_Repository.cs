namespace WeeklyProject03_AssetTracking.Application.Interfaces
{
    using Domain.Entities;

    public interface IAssetRepository
    {
        void Add(Asset asset);
        List<Asset> GetAll();
    }
}
