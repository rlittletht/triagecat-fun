namespace tt.Types.Model.Users;

public class UserPreferences
{
    public int VisibleRows { get; set; }
    public int ImagesPerPage { get; set; }
    public int TileSize { get; set; }

    public Wire.Users.UserPreferences Wire => new()
                                              {
                                                  visibleRows = VisibleRows.ToString(),
                                                  imagesPerPage = ImagesPerPage.ToString(),
                                                  tileSize = TileSize.ToString()
                                              };

    public static UserPreferences CreateFromWire(Wire.Users.UserPreferences wire)
    {
        return new UserPreferences
        {
            VisibleRows = int.Parse(wire.visibleRows),
            ImagesPerPage = int.Parse(wire.imagesPerPage),
            TileSize = int.Parse(wire.tileSize)
        };
    }
}
