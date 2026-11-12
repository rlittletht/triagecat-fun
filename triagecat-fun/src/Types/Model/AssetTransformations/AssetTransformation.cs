namespace tt.Types.Model.AssetTransformations;

public class AssetTransformation
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Transformation { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Wire.AssetTransformations.AssetTransformation Wire => new()
    {
        id = Id.ToString(),
        transformation = Transformation,
        value = Value
    };

    public static AssetTransformation CreateFromWire(Wire.AssetTransformations.AssetTransformation wire)
    {
        return new AssetTransformation
        {
            Id = Guid.Parse(wire.id),
            Transformation = wire.transformation,
            Value = wire.value
        };
    }

    public static Wire.AssetTransformations.AssetTransformation[] CreateFromModelList(AssetTransformations.AssetTransformation[] models)
    {
        Wire.AssetTransformations.AssetTransformation[] transformations = new Wire.AssetTransformations.AssetTransformation[models.Length];
        for (int i = 0; i < models.Length; i++)
        {
            transformations[i] = models[i].Wire;
        }

        return transformations;
    }
    
    public static AssetTransformation[] CreateFromWireList(Wire.AssetTransformations.AssetTransformation[] wires)
    {
        AssetTransformation[] transformations = new AssetTransformation[wires.Length];
        for (int i = 0; i < wires.Length; i++)
        {
            transformations[i] = CreateFromWire(wires[i]);
        }
        return transformations;
    }
}
