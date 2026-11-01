namespace tt.Types.Model.UserAssetStatus;

public class UserAssetStatus
{
    public Guid AssetId { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public bool Triaged { get; set; }

    public Wire.UserAssetStatus.UserAssetStatus Wire => new()
    {
        assetId = AssetId.ToString(),
        userId = UserId.ToString(),
        triaged = Triaged
    };

    public static UserAssetStatus CreateFromWire(Wire.UserAssetStatus.UserAssetStatus wire)
    {
        return new UserAssetStatus
        {
            AssetId = Guid.Parse(wire.assetId),
            UserId = Guid.Parse(wire.userId),
            Triaged = wire.triaged
        };
    }
}
