namespace tt.Types.Model.Assets;

public class AssetEx
{
    public Guid AssetId { get; set; } = Guid.Empty;
    public string Path { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int Priority { get; set; }
    public Guid? AssetGroupId { get; set; }
    public string? AssetGroupName { get; set; }
    public bool Triaged { get; set; }

    public Wire.Assets.AssetEx Wire => new()
    {
        assetId = AssetId.ToString(),
        path = Path,
        notes = Notes,
        priority = Priority.ToString(),
        assetGroupId = AssetGroupId?.ToString(),
        assetGroupName = AssetGroupName,
        triaged = Triaged
    };

    public static AssetEx CreateFromWire(Wire.Assets.AssetEx wire)
    {
        return new AssetEx
        {
            AssetId = Guid.Parse(wire.assetId),
            Path = wire.path,
            Notes = wire.notes,
            Priority = int.Parse(wire.priority),
            AssetGroupId = string.IsNullOrWhiteSpace(wire.assetGroupId) ? null : Guid.Parse(wire.assetGroupId),
            AssetGroupName = wire.assetGroupName,
            Triaged = wire.triaged
        };
    }

    public static List<AssetEx> CreateFromWireList(Wire.Assets.AssetEx[] wires)
    {
        List<AssetEx> assets = new();
        foreach (Wire.Assets.AssetEx wire in wires)
        {
            assets.Add(CreateFromWire(wire));
        }

        return assets;
    }
}
