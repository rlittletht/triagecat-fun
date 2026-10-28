namespace tt.Types.Model.Assets;

public class UpdateAssetParams
{
    public Asset? BaseAsset { get; set; }
    public Asset? UpdatedAsset { get; set; }
    
    public static UpdateAssetParams CreateFromWire(Types.Wire.Assets.UpdateAssetParams wireParams)
    {
        if (wireParams.baseAsset == null || wireParams.updatedAsset == null)
        {
            throw new ArgumentException("Both baseAsset and updatedAsset must be provided.");
        }
        
        return new UpdateAssetParams()
        {
            BaseAsset = Asset.CreateFromWire(wireParams.baseAsset),
            UpdatedAsset = Asset.CreateFromWire(wireParams.updatedAsset)
        };
    }
}