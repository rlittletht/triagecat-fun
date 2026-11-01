
namespace tt.Types.Model.Assets;

public class Asset
{
    public Guid AssetId { get; set; } = Guid.Empty;
    public string Path { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public Guid? AssetGroupId { get; set; }
    public string? AssetGroupName { get; set; }

    public Wire.Assets.Asset Wire => new()
    {
        assetId = AssetId.ToString(),
        path = Path,
        notes = Notes,
        priority = Priority.ToString(),
        assetGroupId = AssetGroupId?.ToString(),
        assetGroupName = AssetGroupName
    };

    public static Asset CreateFromWire(Wire.Assets.Asset wire)
    {
        return new Asset
        {
            AssetId = Guid.Parse(wire.assetId),
            Path = wire.path,
            Notes = wire.notes,
            Priority = int.Parse(wire.priority),
            AssetGroupId = string.IsNullOrWhiteSpace(wire.assetGroupId) ? null : Guid.Parse(wire.assetGroupId),
            AssetGroupName = wire.assetGroupName
        };
    }

    public static List<Asset> CreateFromWireList(Wire.Assets.Asset[] wires)
    {
        List<Asset> assets = new List<Asset>();
        foreach (Wire.Assets.Asset wire in wires)
        {
            assets.Add(CreateFromWire(wire));
        }

        return assets;
    }
}