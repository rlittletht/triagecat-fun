namespace tt.Types.Model.AssetGroups;

public class CreateAssetGroupParams
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Types.Wire.AssetGroups.CreateAssetGroupParams Wire => new() { name = Name, description = Description };

    public static CreateAssetGroupParams CreateFromWire(Types.Wire.AssetGroups.CreateAssetGroupParams wire)
    {
        return new() { Name = wire.name, Description = wire.description };
    }
}