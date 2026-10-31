namespace tt.Types.Model;

// these are just hints for the UI - claims will be checked on the server before every operation
public class UiVisibility
{
    // show database repair/admin
    public bool ShowDatabaseRepair { get; set; } = false;

    // show the add assets UI
    public bool ShowAddAssets { get; set; } = false;

    // allow reset password without providing old password
    public bool AllowResetPassword { get; set; } = false;

    public Types.Wire.UiVisibility Wire =>
        new()
        {
            showDatabaseRepair = ShowDatabaseRepair,
            showAddAssets = ShowAddAssets,
            allowResetPassword = AllowResetPassword
        };
}
