using Microsoft.IdentityModel.Tokens;

namespace tt.Types.Model.Users;

public class AuthUser
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}