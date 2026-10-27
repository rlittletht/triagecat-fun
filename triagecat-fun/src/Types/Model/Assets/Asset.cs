
namespace tt.Types.Model.Assets;

public class Asset
{
    public Guid AssetId { get; set; } = Guid.Empty;
    public string Path { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;

    public Wire.Assets.Asset Wire => new() { assetId = AssetId.ToString(), path = Path, notes = Notes, priority = Priority.ToString() };

    public static Asset CreateFromWire(Wire.Assets.Asset wire)
    {
        return new Asset
        {
            AssetId = Guid.Parse(wire.assetId),
            Path = wire.path,
            Notes = wire.notes,
            Priority = int.Parse(wire.priority)
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