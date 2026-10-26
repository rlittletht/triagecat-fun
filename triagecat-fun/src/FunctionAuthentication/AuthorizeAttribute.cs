
namespace tt.FunctionAuthentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute: Attribute
{
    public bool Value { get; }

    public AuthorizeAttribute(bool value = true)
    {
        Value = value;
    }

    public string[]? RequiredClaims { get; set; }
    public string[]? RequiredClaimValues { get; set; }
}
