using Microsoft.Extensions.Logging;
using TCore.Logging;
using tt.Types.Model;
using tt.Types.Model.AssetTransformations;

namespace tt.Model;

public class AssetTransformations
{
    public static int SetAssetTransformation(CorrelationID crid, ILogger? logger, SetAssetTransformationParam setParams)
    {
        if (setParams.AssetTransformation == null)
        {
            throw new TtExceptionInvalidArgument("assetTransformation must be provided.");
        }

        Session.RexDatabase.SetAssetTransformation(crid, setParams.AssetTransformation);
        return 1;
    }
}
