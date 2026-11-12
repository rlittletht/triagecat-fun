namespace tt.Types.Model.AssetTransformations;

public class SetAssetTransformationParam
{
    public AssetTransformation? AssetTransformation { get; set; }

    public static SetAssetTransformationParam CreateFromWire(Types.Wire.AssetTransformations.SetAssetTransformationParam wire)
    {
        return new SetAssetTransformationParam
        {
            AssetTransformation = wire.assetTransformation == null ? null : AssetTransformation.CreateFromWire(wire.assetTransformation)
        };
    }
}
