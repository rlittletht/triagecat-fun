namespace tt.Types.Model.AssetGroups;

public class AssetGroup
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Wire.AssetGroups.AssetGroup Wire => new()
    {
        id = Id.ToString(),
        name = Name,
        description = Description
    };

    public static AssetGroup CreateFromWire(Wire.AssetGroups.AssetGroup wire)
    {
        return new AssetGroup
        {
            Id = Guid.Parse(wire.id),
            Name = wire.name,
            Description = wire.description
        };
    }
}
