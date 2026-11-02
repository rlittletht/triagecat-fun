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

    public static IReadOnlyCollection<Types.Wire.Assets.AssetEx> QueryAssets(
        CorrelationID crid, ILogger? logger, Guid userId, Types.Model.Assets.QueryAsset? queryAsset)
    {
        bool? triaged = queryAsset?.Triaged;
        string? assetGroupName = queryAsset?.AssetGroupName;
        bool? trashed = queryAsset?.Trashed;

        IReadOnlyCollection<Types.Model.Assets.AssetEx> assets = Session.RexDatabase.QueryAssets(crid, userId, triaged, trashed, assetGroupName);
        List<Types.Wire.Assets.AssetEx> wireAssets = new();

        foreach (Types.Model.Assets.AssetEx asset in assets)
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
                    AssetGroup newGroup = new() { Name = addAsset.AssetGroupName.Trim(), Description = String.Empty };
                    newGroup.Id = Session.RexDatabase.CreateAssetGroup(crid, newGroup);

                    assetGroupsByName[newGroup.Name] = newGroup;
                    assetGroup = newGroup;
                }

                assetGroupId = assetGroup.Id;
            }

            Asset asset = new()
                          {
                              AssetId = RT.Comb.Provider.Sql.Create(),
                              Path = addAsset.Path,
                              Notes = string.Empty,
                              Priority = 0,
                              AssetGroupId = assetGroupId
                          };
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

    public static Dictionary<string, object?> BuildUpdateListFromBaseAndNewUserAssetStatus(
        Types.Model.Assets.UserAssetStatus baseStatus, Types.Model.Assets.UserAssetStatus newStatus)
    {
        Dictionary<string, object?> updates = new Dictionary<string, object?>();

        if (baseStatus.Triaged != newStatus.Triaged)
        {
            updates["Triaged"] = newStatus.Triaged;
        }

        if (baseStatus.Trashed != newStatus.Trashed)
        {
            updates["Trashed"] = newStatus.Trashed;
        }

        return updates;
    }

    public static int UpdateUserAssetStatus(CorrelationID crid, ILogger? logger, UpdateUserAssetStatusParams updateParams)
    {
        if (updateParams.BaseUserAssetStatus == null || updateParams.UpdatedUserAssetStatus == null)
        {
            throw new TtExceptionInvalidArgument("Both BaseUserAssetStatus and UpdatedUserAssetStatus must be provided.");
        }

        if (updateParams.BaseUserAssetStatus.AssetId != updateParams.UpdatedUserAssetStatus.AssetId)
        {
            throw new TtExceptionInvalidArgument("Asset IDs do not match.");
        }

        if (updateParams.BaseUserAssetStatus.UserId != updateParams.UpdatedUserAssetStatus.UserId)
        {
            throw new TtExceptionInvalidArgument("User IDs do not match.");
        }

        try
            // first, make sure the assert status exists
        {
            UserAssetStatus existingStatus = Session.RexDatabase.GetUserAssetStatus(
                crid,
                updateParams.BaseUserAssetStatus.AssetId,
                updateParams.BaseUserAssetStatus.UserId);

            // just update, ignore coherency failure
            Dictionary<string, object?> updates = BuildUpdateListFromBaseAndNewUserAssetStatus(
                updateParams.BaseUserAssetStatus,
                updateParams.UpdatedUserAssetStatus);

            Session.RexDatabase.UpdateUserAssetStatus(crid, updateParams.BaseUserAssetStatus.AssetId, updateParams.BaseUserAssetStatus.UserId, updates);
            return updates.Count;
        }
        catch (TCore.SqlCore.SqlExceptionNoResults)
        {
            // need to create
            Session.RexDatabase.CreateUserAssetStatus(crid, updateParams.UpdatedUserAssetStatus);
            return 1;
        }
    }

    public static int UpdateAssetInfoEx(CorrelationID crid, ILogger? logger, Guid userId, UpdateAssetInfoExParams updateParams)
    {
        if (updateParams.BaseAsset == null || updateParams.UpdatedAsset == null)
        {
            throw new TtExceptionInvalidArgument("Both BaseAsset and UpdatedAsset must be provided.");
        }

        if (updateParams.BaseAsset.AssetId != updateParams.UpdatedAsset.AssetId)
        {
            throw new TtExceptionInvalidArgument("Asset IDs do not match.");
        }

        Asset baseAsset = new()
                          {
                              AssetId = updateParams.BaseAsset.AssetId,
                              Path = updateParams.BaseAsset.Path,
                              Notes = updateParams.BaseAsset.Notes,
                              Priority = updateParams.BaseAsset.Priority,
                              AssetGroupId = updateParams.BaseAsset.AssetGroupId,
                              AssetGroupName = updateParams.BaseAsset.AssetGroupName
                          };

        Asset updatedAsset = new()
                             {
                                 AssetId = updateParams.UpdatedAsset.AssetId,
                                 Path = updateParams.UpdatedAsset.Path,
                                 Notes = updateParams.UpdatedAsset.Notes,
                                 Priority = updateParams.UpdatedAsset.Priority,
                                 AssetGroupId = updateParams.UpdatedAsset.AssetGroupId,
                                 AssetGroupName = updateParams.UpdatedAsset.AssetGroupName
                             };

        Dictionary<string, object?> updates = BuildUpdateListFromBaseAndNew(baseAsset, updatedAsset);
        int updateCount = updates.Count;

        if (updates.Count > 0)
        {
            Session.RexDatabase.UpdateAssetItemValues(crid, baseAsset.AssetId, updates);
        }

        if (updateParams.BaseAsset.Triaged != updateParams.UpdatedAsset.Triaged)
        {
            UserAssetStatus userAssetStatusBase = new()
                                                  {
                                                      AssetId = baseAsset.AssetId,
                                                      UserId = userId,
                                                      Triaged = updateParams.BaseAsset.Triaged,
                                                      Trashed = updateParams.BaseAsset.Trashed
                                                  };

            UserAssetStatus userAssetStatusUpdated = new()
                                                     {
                                                         AssetId = updatedAsset.AssetId,
                                                         UserId = userId,
                                                         Triaged = updateParams.UpdatedAsset.Triaged,
                                                         Trashed = updateParams.UpdatedAsset.Trashed
                                                     };

            UpdateUserAssetStatus(
                crid,
                logger,
                new UpdateUserAssetStatusParams() { BaseUserAssetStatus = userAssetStatusBase, UpdatedUserAssetStatus = userAssetStatusUpdated });

            updateCount++;
        }

        return updateCount;
    }
}