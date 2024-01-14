using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleGrasshopper.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace GHRadialMenu.Views.ViewModels;

internal partial class ShortcutEditorViewModel : ObservableObject
{
    private readonly Shortcut[] _lastShortcuts = Data.Shortcuts;

    [ObservableProperty]
    ObservableCollection<ShortcutViewModel> _shortcuts
        = new(Data.Shortcuts.Select(s => new ShortcutViewModel(s)));

    public Shortcut[] NowShortcuts => [.. Shortcuts];

    [RelayCommand]
    public void Apply()
    {
        Data.Shortcuts = NowShortcuts;
    }

    [RelayCommand]
    public void Cancel()
    {
        Data.Shortcuts = _lastShortcuts;
        Shortcuts = new(_lastShortcuts.Select(s => new ShortcutViewModel(s)));
    }

    [RelayCommand]
    public void Copy()
    {
        Clipboard.SetText(IOHelper.SerializeObject(NowShortcuts));
    }

    [RelayCommand]
    public void Paste()
    {
        var str = Clipboard.GetText();
        var newShortcuts = IOHelper.DeserializeObject<Shortcut[]>(str);

        if (newShortcuts == null) return;

        foreach (var shortcut in newShortcuts)
        {
            ShortcutViewModel? removedShortcut = null;
            foreach (var eshortCut in Shortcuts)
            {
                if (eshortCut.FirstKey == shortcut.FirstKey
                    && eshortCut.SecondKey == shortcut.SecondKey)
                {
                    removedShortcut = eshortCut;
                    break;
                }
            }
            if (removedShortcut is not null)
            {
                Shortcuts.Remove(removedShortcut);
            }
            Shortcuts.Add(new(shortcut));
        }
    }
}
