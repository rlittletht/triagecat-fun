namespace tt.Types.Wire.Assets;

public class UserAssetStatus
{
    public string assetId { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
    public bool triaged { get; set; }
    public bool trashed { get; set; }
}
