using tt.Types.Wire.Assets;

namespace tt.Types.Wire.Assets;

public class UpdateUserAssetStatusParams
{
    public UserAssetStatus? baseUserAssetStatus { get; set; }
    public UserAssetStatus? updatedUserAssetStatus { get; set; }
}
