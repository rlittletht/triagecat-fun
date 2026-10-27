using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using TCore.Logging;
using tt.Types.Model.Assets;

namespace tt.Model;

public class Assets
{
#region Get

    public static IReadOnlyCollection<Types.Wire.Assets.Asset> GetAllAssets(CorrelationID crid, ILogger? logger)
    {
        List<Types.Wire.Assets.Asset> wireAssets = new List<Types.Wire.Assets.Asset>();

        IReadOnlyCollection<Types.Model.Assets.Asset> assets = Session.RexDatabase.GetAllAssets(crid);

        foreach (Types.Model.Assets.Asset asset in assets)
        {
            wireAssets.Add(asset.Wire);
        }

        return wireAssets;
    }

#endregion

    public static int AddAssets(CorrelationID crid, ILogger? logger, IEnumerable<Types.Model.Assets.AddAsset> addAssets)
    {
        List<Asset> assets = new();

        foreach (AddAsset addAsset in addAssets)
        {
            Asset asset = new() { AssetId = RT.Comb.Provider.Sql.Create(), Path = addAsset.Path, Notes = string.Empty, Priority = 0 };
            assets.Add(asset);
        }

        return Session.RexDatabase.AddAssets(crid, assets);
    }
}