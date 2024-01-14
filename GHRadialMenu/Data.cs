using GHRadialMenu.Actions;
using GHRadialMenu.Views;
using SimpleGrasshopper.Attributes;
using SimpleGrasshopper.Util;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GHRadialMenu;
internal static partial class Data
{
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

    [Setting]
    private static readonly Shortcut[] _shortcuts =
    [
        new()
        {
            Name = "Preview\nEnable",
            FirstKey = Keys.P,
            SecondKey = Keys.I,
            Actions =
            [
                new MenuItemAction("Solution", "Preview Selected On"),
                new MenuItemAction("Solution", "Preview Selected Off"),
                new MenuItemAction("Solution", "Toggle Preview"),
                new MenuItemAction("Solution", "Enable Selected"),
                new MenuItemAction("Solution", "Disable Selected"),
                new MenuItemAction("Solution", "Toggle Enable Selected"),
            ],
        },
        new()
        {
            Name = "Points",
            FirstKey = Keys.P,
            SecondKey = Keys.P,
            Actions =
            [
                new NewObjectAction(new("3581f42a-9592-4549-bd6b-1c0fc39d067b")),
                new NewObjectAction(new("571ca323-6e55-425a-bf9e-ee103c7ba4b9")),
                new NewObjectAction(new("6eaffbb2-3392-441a-8556-2dc126aa8910")),
            ],
        },
    ];

    public static void PasteFromClipboard()
    {
        var str = Clipboard.GetText();
        var newShortcuts = IOHelper.DeserializeObject<Shortcut[]>(str);

        if (newShortcuts == null) return;

        var oldShortcuts = Shortcuts;
        var result = new List<Shortcut>(newShortcuts.Length + oldShortcuts.Length);
        result.AddRange(oldShortcuts);

        foreach (var shortcut in newShortcuts )
        {
            Shortcut? removedShortcut = null;
            foreach (var eshortCut in result)
            {
                if(eshortCut.FirstKey == shortcut.FirstKey
                    && eshortCut.SecondKey == shortcut.SecondKey)
                {
                    removedShortcut = eshortCut;
                    break;
                }
            }
            if (removedShortcut is not null)
            {
                result.Remove(removedShortcut);
            }
            result.Add(shortcut);
        }
        Shortcuts = [.. result];
    }
}
