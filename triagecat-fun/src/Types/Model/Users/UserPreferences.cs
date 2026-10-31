namespace tt.Types.Model.Users;

public class UserPreferences
{
    public int ImagesPerPage { get; set; }
    public int TileSize { get; set; }

    public Wire.Users.UserPreferences Wire => new()
                                              {
                                                  imagesPerPage = ImagesPerPage.ToString(),
                                                  tileSize = TileSize.ToString()
                                              };

    public static UserPreferences CreateFromWire(Wire.Users.UserPreferences wire)
    {
        return new UserPreferences
        {
            ImagesPerPage = int.Parse(wire.imagesPerPage),
            TileSize = int.Parse(wire.tileSize)
        };
    }
}
