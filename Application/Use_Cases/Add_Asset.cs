using System;
using System.Collections.Generic;
using System.Text;

using WeeklyProject03_AssetTracking.Domain.Entities;
using WeeklyProject03_AssetTracking.Application.Interfaces;

namespace WeeklyProject03_AssetTracking.Application.Use_Cases
{
    public class AddAsset
    {
        private readonly IAssetRepository _repo;

        public AddAsset(IAssetRepository repo)
        {
            _repo = repo;
        }

        public void Execute(Asset asset)
        {
            _repo.Add(asset);
        }
    }
}
