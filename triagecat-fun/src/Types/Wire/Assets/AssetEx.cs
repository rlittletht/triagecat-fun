namespace tt.Types.Wire.Assets;

public class AssetEx
{
    public string assetId { get; set; } = string.Empty;
    public string path { get; set; } = string.Empty;
    public string notes { get; set; } = string.Empty;
    public string priority { get; set; } = string.Empty;
    public string? assetGroupId { get; set; }
    public string? assetGroupName { get; set; }
    public bool triaged { get; set; }
    public bool trashed { get; set; }
    public Wire.AssetTransformations.AssetTransformation[] assetTransformations { get; set; } = Array.Empty<Wire.AssetTransformations.AssetTransformation>();
}
