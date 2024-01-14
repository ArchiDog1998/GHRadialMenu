using GHRadialMenu.Actions;
using System.Windows.Forms;

namespace GHRadialMenu;

public class Shortcut
{
    public string Name { get; set; } = "Unnamed";
    public Keys FirstKey { get; set; } = Keys.None;
    public Keys SecondKey { get; set; } = Keys.None;
    public IAction[] Actions { get; set; } = [];

    public bool IsTriggered(Keys lastKey, Keys nowKey)
    {
        if (FirstKey == Keys.None) return false;
        if (SecondKey == Keys.None)
        {
            return nowKey == FirstKey;
        }
        else
        {
            return lastKey == FirstKey && nowKey == SecondKey;
        }
    }
}
