using Microsoft.Extensions.Logging;
using TCore.Logging;
using tt.Types.Model.AssetGroups;

namespace tt.Model;

public class AssetGroups
{
    public static void CreateAssetGroup(CorrelationID crid, ILogger? logger, AssetGroup assetGroup)
    {
        Session.RexDatabase.CreateAssetGroup(crid, assetGroup);
    }

    public static Types.Wire.AssetGroups.AssetGroup GetAssetGroup(CorrelationID crid, ILogger? logger, Guid id)
    {
        AssetGroup assetGroup = Session.RexDatabase.GetAssetGroup(crid, id);
        return assetGroup.Wire;
    }

    public static IReadOnlyCollection<Types.Wire.AssetGroups.AssetGroup> GetAllAssetGroups(CorrelationID crid, ILogger? logger)
    {
        IReadOnlyCollection<AssetGroup> assetGroups = Session.RexDatabase.GetAllAssetGroups(crid);
        List<Types.Wire.AssetGroups.AssetGroup> wireGroups = new();

        foreach (AssetGroup assetGroup in assetGroups)
        {
            wireGroups.Add(assetGroup.Wire);
        }

        return wireGroups;
    }
    
    public static Types.Wire.AssetGroups.AssetGroup CreateAssetGroup(CorrelationID crid, ILogger? logger, CreateAssetGroupParams createParams)
    {
        AssetGroup assetGroup = new() { Name = createParams.Name, Description = createParams.Description };
        Guid guid = Session.RexDatabase.CreateAssetGroup(crid, assetGroup);

        assetGroup.Id = guid;
        
        return assetGroup.Wire;
    }
}
