using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using TCore.Logging;
using tt.Types.Model.AssetGroups;
using tt.Types.Model;
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
        IReadOnlyCollection<AssetGroup> assetGroups = Session.RexDatabase.GetAllAssetGroups(crid);
        Dictionary<string, AssetGroup> assetGroupsByName = new(StringComparer.OrdinalIgnoreCase);

        foreach (AssetGroup assetGroup in assetGroups)
        {
            if (!assetGroupsByName.ContainsKey(assetGroup.Name))
            {
                assetGroupsByName[assetGroup.Name] = assetGroup;
            }
        }
        
        foreach (AddAsset addAsset in addAssets)
        {
            Guid? assetGroupId = null;
            string? assetGroupName = addAsset.AssetGroupName;

            if (!string.IsNullOrWhiteSpace(addAsset.AssetGroupName))
            {
                if (!assetGroupsByName.TryGetValue(addAsset.AssetGroupName.Trim(), out AssetGroup? assetGroup))
                {
                    AssetGroup newGroup = new()
                                          {
                                              Name = addAsset.AssetGroupName.Trim(),
                                              Description = String.Empty
                                          };
                    newGroup.Id = Session.RexDatabase.CreateAssetGroup(crid, newGroup);

                    assetGroupsByName[newGroup.Name] = newGroup;
                    assetGroup = newGroup;
                }

                assetGroupId = assetGroup.Id;
            }

            Asset asset = new() { AssetId = RT.Comb.Provider.Sql.Create(), Path = addAsset.Path, Notes = string.Empty, Priority = 0, AssetGroupId = assetGroupId };
            assets.Add(asset);
        }

        return Session.RexDatabase.AddAssets(crid, assets);
    }

    public static Dictionary<string, object?> BuildUpdateListFromBaseAndNew(Types.Model.Assets.Asset baseAsset, Types.Model.Assets.Asset newAsset)
    {
        Dictionary<string, object?> updates = new Dictionary<string, object?>();

        if (baseAsset.Path != newAsset.Path)
        {
            updates["Path"] = newAsset.Path;
        }

        if (baseAsset.Notes != newAsset.Notes)
        {
            updates["Notes"] = newAsset.Notes;
        }

        if (baseAsset.Priority != newAsset.Priority)
        {
            updates["Priority"] = newAsset.Priority;
        }

        if (baseAsset.AssetGroupId != newAsset.AssetGroupId)
        {
            updates["assetgroupid"] = newAsset.AssetGroupId?.ToString() ?? (object)DBNull.Value;
        }

        return updates;
    }

    public static int UpdateAsset(CorrelationID crid, ILogger? logger, Types.Model.Assets.UpdateAssetParams updateParams)
    {
        if (updateParams.BaseAsset == null || updateParams.UpdatedAsset == null)
        {
            throw new TtExceptionInvalidArgument("Both BaseAsset and UpdatedAsset must be provided.");
        }

        if (updateParams.BaseAsset.AssetId != updateParams.UpdatedAsset.AssetId)
        {
            throw new TtExceptionInvalidArgument("Asset IDs do not match.");
        }

        Dictionary<string, object?> updates = BuildUpdateListFromBaseAndNew(updateParams.BaseAsset!, updateParams.UpdatedAsset!);
        
        if (updates.Count > 0)
        {
            Session.RexDatabase.UpdateAssetItemValues(crid, updateParams.BaseAsset!.AssetId, updates);
        }

        return updates.Count;
    }
}