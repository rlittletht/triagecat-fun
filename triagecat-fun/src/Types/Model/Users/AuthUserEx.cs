using Microsoft.IdentityModel.Tokens;
using tt.Types.Wire;

namespace tt.Types.Model.Users;

public class AuthUserEx
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Claim { get; set; }
    public string? ClaimValue { get; set; }
}