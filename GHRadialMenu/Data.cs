using GHRadialMenu.Actions;
using GHRadialMenu.Views;
using SimpleGrasshopper.Attributes;
using SimpleGrasshopper.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GHRadialMenu;
internal static partial class Data
{
    public static readonly Bitmap Icon = typeof(Data).Assembly.GetBitmap("RadialMenuIcon.png")!;

    internal static ShortcutEditor? _editor;

    [Config("Radial Menu", "Edit the shortcuts")]
    public static object Edit
    {
        get => true;
        set
        {
            if (_editor != null)
            {
                _editor.Focus();
            }
            else
            {
                _editor = new ShortcutEditor();
                _editor.Show();
            }
        }
    }

    private static Shortcut[] GetDefaultShortcuts()
    {
        try
        {
            return IOHelper.DeserializeObject<Shortcut[]>(
                typeof(Data).Assembly.GetString("https://raw.githubusercontent.com/ArchiDog1998/GHRadialMenu/main/Resources/DefaultShortcuts.json") ?? string.Empty) ?? [];
        }
        catch
        {
            return [];
        }
    }

    [Setting]
    private static readonly Shortcut[] _shortcuts = GetDefaultShortcuts();
}
