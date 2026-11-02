namespace tt.Types.Model.Assets;

public class QueryAsset
{
    public bool? Triaged { get; set; }
    public bool? Trashed { get; set; }
    public string? AssetGroupName { get; set; }

    public static QueryAsset CreateFromWire(Types.Wire.Assets.QueryAsset wire)
    {
        bool? triaged = null;
        bool? trashed = null;

        if (!string.IsNullOrWhiteSpace(wire.triaged))
        {
            triaged = bool.Parse(wire.triaged);
        }

        if (!string.IsNullOrWhiteSpace(wire.trashed))
        {
            trashed = bool.Parse(wire.trashed);
        }

        return new QueryAsset
        {
            Triaged = triaged,
            Trashed = trashed,
            AssetGroupName = string.IsNullOrWhiteSpace(wire.assetGroupName) ? null : wire.assetGroupName.Trim()
        };
    }
}
