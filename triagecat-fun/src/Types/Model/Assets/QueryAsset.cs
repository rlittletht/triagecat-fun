namespace tt.Types.Model.Assets;

public class QueryAsset
{
    public bool? Triaged { get; set; }
    public string? AssetGroupName { get; set; }

    public static QueryAsset CreateFromWire(Types.Wire.Assets.QueryAsset wire)
    {
        bool? triaged = null;

        if (!string.IsNullOrWhiteSpace(wire.triaged))
        {
            triaged = bool.Parse(wire.triaged);
        }

        return new QueryAsset
        {
            Triaged = triaged,
            AssetGroupName = string.IsNullOrWhiteSpace(wire.assetGroupName) ? null : wire.assetGroupName.Trim()
        };
    }
}
