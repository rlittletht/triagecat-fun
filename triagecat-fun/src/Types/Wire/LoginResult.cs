
namespace tt.Types.Wire;

public class LoginResult : UiVisibility
{
    public string email { get; set; } = string.Empty;
    public string displayName { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
}
