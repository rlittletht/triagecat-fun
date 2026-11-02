namespace tt.Types.Model.Users;

public class UserPreferences
{
    public int ImagesPerPage { get; set; }
    public int TileSize { get; set; }
    public string DefaultFilter { get; set; } = string.Empty;
    
    public Wire.Users.UserPreferences Wire => new()
                                              {
                                                  imagesPerPage = ImagesPerPage.ToString(),
                                                  tileSize = TileSize.ToString(),
                                                  defaultFilter = DefaultFilter
                                              };

    public static UserPreferences CreateFromWire(Wire.Users.UserPreferences wire)
    {
        return new UserPreferences
        {
            ImagesPerPage = int.Parse(wire.imagesPerPage),
            TileSize = int.Parse(wire.tileSize),
            DefaultFilter = wire.defaultFilter
        };
    }
}
