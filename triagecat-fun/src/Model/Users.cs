using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TCore.Logging;
using tt.Types.Model.Auth;
using tt.Types.Model;
using tt.Types.Model.Users;

namespace tt.Model;

public class Users
{
    public static Types.Wire.Users.UserPreferences GetUserPreferences(CorrelationID crid, ILogger? logger, Guid userId)
    {
        UserPreferences userPreferences = Session.RexDatabase.GetUserPreferences(crid, userId);
        return userPreferences.Wire;
    }

    public static Dictionary<string, object?> BuildUpdateListFromBaseAndNew(UserPreferences baseUserPreferences, UserPreferences newUserPreferences)
    {
        Dictionary<string, object?> updates = new Dictionary<string, object?>();

        if (baseUserPreferences.VisibleRows != newUserPreferences.VisibleRows)
        {
            updates["visibleRows"] = newUserPreferences.VisibleRows;
        }

        if (baseUserPreferences.ImagesPerPage != newUserPreferences.ImagesPerPage)
        {
            updates["imagesPerPage"] = newUserPreferences.ImagesPerPage;
        }

        return updates;
    }

    public static int UpdateUserPreferences(CorrelationID crid, ILogger? logger, Guid userId, UpdateUserPreferencesParams updateParams)
    {
        if (updateParams.BaseUserPreferences == null || updateParams.UpdatedUserPreferences == null)
        {
            throw new TtExceptionInvalidArgument("Both BaseUserPreferences and UpdatedUserPreferences must be provided.");
        }

        Dictionary<string, object?> updates = BuildUpdateListFromBaseAndNew(updateParams.BaseUserPreferences, updateParams.UpdatedUserPreferences);

        if (updates.Count > 0)
        {
            Session.RexDatabase.UpdateUserPreferences(crid, userId, updates);
        }

        return updates.Count;
    }
}