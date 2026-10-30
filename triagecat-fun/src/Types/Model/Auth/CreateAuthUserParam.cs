namespace tt.Types.Model.Auth;

public class CreateAuthUserParam
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    public static CreateAuthUserParam CreateFromWire(Types.Wire.Auth.CreateAuthUserParam wire)
    {
        return new CreateAuthUserParam
        {
            Email = wire.email,
            Password = wire.password
        };
    }
}