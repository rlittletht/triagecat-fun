namespace tt.Types.Model.Assets;

public class UserAssetStatus
{
    public Guid AssetId { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public bool Triaged { get; set; }

    public Wire.Assets.UserAssetStatus Wire => new()
    {
        assetId = AssetId.ToString(),
        userId = UserId.ToString(),
        triaged = Triaged
    };

    public static UserAssetStatus CreateFromWire(Wire.Assets.UserAssetStatus wire)
    {
        return new UserAssetStatus
        {
            AssetId = Guid.Parse(wire.assetId),
            UserId = Guid.Parse(wire.userId),
            Triaged = wire.triaged
        };
    }
}
