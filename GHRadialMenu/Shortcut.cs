using GHRadialMenu.Actions;
using System.Windows.Forms;

namespace GHRadialMenu;

public class Shortcut
{
    public string Name { get; set; } = "Unnamed";
    public Keys PrimaryKey { get; set; } = Keys.None;
    public Keys SecondaryKey { get; set; } = Keys.None;
    public IAction[] Actions { get; set; } = [];

    public bool IsTriggered(Keys lastKey, Keys nowKey)
    {
        if (PrimaryKey == Keys.None) return false;
        if (SecondaryKey == Keys.None)
        {
            return nowKey == PrimaryKey;
        }
        else
        {
            return lastKey == PrimaryKey && nowKey == SecondaryKey;
        }
    }
}
