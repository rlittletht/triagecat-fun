using System.Net.NetworkInformation;
using tt.Types.Model.Assets;

namespace tt.Types.Model.Assets;

public class UpdateUserAssetStatusParams
{
    public UserAssetStatus? BaseUserAssetStatus { get; set; }
    public UserAssetStatus? UpdatedUserAssetStatus { get; set; }
    
    public static UpdateUserAssetStatusParams CreateFromWire(Types.Wire.Assets.UpdateUserAssetStatusParams wireParams)
    {
        return new UpdateUserAssetStatusParams
        {
            BaseUserAssetStatus = UserAssetStatus.CreateFromWire(wireParams.baseUserAssetStatus!),
            UpdatedUserAssetStatus = UserAssetStatus.CreateFromWire(wireParams.updatedUserAssetStatus!)
        };
    }
}
