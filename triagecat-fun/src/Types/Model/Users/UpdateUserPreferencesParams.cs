namespace tt.Types.Model.Users;

public class UpdateUserPreferencesParams
{
    public UserPreferences? BaseUserPreferences { get; set; }
    public UserPreferences? UpdatedUserPreferences { get; set; }

    public static UpdateUserPreferencesParams CreateFromWire(Types.Wire.Users.UpdateUserPreferencesParams wireParams)
    {
        if (wireParams.baseUserPreferences == null || wireParams.updatedUserPreferences == null)
        {
            throw new ArgumentException("Both baseUserPreferences and updatedUserPreferences must be provided.");
        }

        return new UpdateUserPreferencesParams
        {
            BaseUserPreferences = UserPreferences.CreateFromWire(wireParams.baseUserPreferences),
            UpdatedUserPreferences = UserPreferences.CreateFromWire(wireParams.updatedUserPreferences)
        };
    }
}
