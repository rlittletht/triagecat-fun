namespace tt.Types.Model.Auth;

public class CreateAuthUserResult
{
    public string UserId { get; set; } = string.Empty;
    
    public Types.Wire.Auth.CreateAuthUserResult Wire => new ()
    {
        userId = UserId
    };
}