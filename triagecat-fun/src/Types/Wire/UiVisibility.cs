namespace tt.Types.Wire;

public class UiVisibility
{
    // show database repair/admin
    public bool showDatabaseRepair { get; set; } = false;

    // allow user to reset password without providing old password
    public bool allowResetPassword { get; set; } = false;
    
    // show the add assets UI
    public bool showAddAssets { get; set; } = false;
}
