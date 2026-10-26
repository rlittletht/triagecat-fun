using tt.Database;
using tt.Types.Model;

namespace tt.Model;

/*----------------------------------------------------------------------------
    %%Class: Session

    Holds all static session values that don't have a better home (like Auth)
----------------------------------------------------------------------------*/
public class Session
{
    private static IRexDatabase? m_database = null;

    public static IRexDatabase RexDatabase
    {
        get => m_database ?? throw new TtExceptionNotInitializedError("RexDatabase");
        set => m_database = value;
    }
}
