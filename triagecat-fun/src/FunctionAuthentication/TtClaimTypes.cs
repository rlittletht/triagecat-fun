using System.Xml;

namespace tt.Controllers;

public static class TtClaimTypes
{
    // Claims

    public const string UserId = "https://www.thetasoft.com/claimTypes/userId";

    public const string GroupId = "https://www.thetasoft.com/claimTypes/groupId";

    public const string DatabaseRepair = "https://www.thetasoft.com/claimTypes/databaseRepair";

    // this claim allows the current authenticated user to reset their password without
    // providing the old password (used in one-time key password reset or joining via invitation)
    public const string ResetPassword = "https://www.thetasoft.com/claimTypes/resetPassword";

    // User can create new tests (which includes creating a new test user and supporting database
    // objects and permissions)
    public const string CreateTests = "https://www.thetasoft.com/claimTypes/createTests";

    // asset access is an array: "read", "write"
    public const string AssetAccess = "https://www.thetasoft.com/claimTypes/assetAccess";
    
    // some claims are just present or not; this is the value that must be set for it to be considered
    // "present"
    public const string ClaimPresent = "true";

    // Claims Values
    public const string Access_Read = "read";
    public const string Access_Write = "write";

    public static HashSet<string> Access_Claims = new() { Access_Read, Access_Write };

    public static readonly Guid s_RexGroupId = Guid.Empty;
}