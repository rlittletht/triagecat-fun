namespace tt.Types.Model.Assets;

public class AddAsset
{
    public string Path { get; set; } = string.Empty;

    public static AddAsset CreateFromWire(Types.Wire.Assets.AddAsset wire)
    {
        return new() { Path = wire.path };
    }

    public static List<AddAsset> CreateFromWireList(IEnumerable<Types.Wire.Assets.AddAsset> wireList)
    {
        List<AddAsset> modelAssets = new();
        foreach (Wire.Assets.AddAsset wire in wireList)
        {
            modelAssets.Add(CreateFromWire(wire));
        }
        return modelAssets;
    }
}