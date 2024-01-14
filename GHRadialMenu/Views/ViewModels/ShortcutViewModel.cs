using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GHRadialMenu.Actions;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace GHRadialMenu.Views.ViewModels;
internal partial class ShortcutViewModel(Shortcut shortcut) : ObservableObject
{
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedCommand))]
    [ObservableProperty]
    private int _selected = -1;

    public bool CanRemove => Selected >= 0;

    [ObservableProperty]
    private string _name = shortcut.Name;

    [ObservableProperty]
    private Keys _firstKey = shortcut.FirstKey;

    [ObservableProperty]
    private Keys _secondKey = shortcut.SecondKey;

    [ObservableProperty]
    private ObservableCollection<IAction> _actions = new(shortcut.Actions);

    public ShortcutViewModel() : this(new Shortcut())
    {
        
    }

    public static implicit operator Shortcut(ShortcutViewModel model) => new()
    {
        Name = model.Name,
        FirstKey = model.FirstKey,
        SecondKey = model.SecondKey,
        Actions = [.. model.Actions],
    };

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void RemoveSelected()
    {
        if(Selected >= 0)
        {
            Actions.RemoveAt(Selected);
        }
    }

    [RelayCommand]
    private void AddAction()
    {
        var window = new AddActionsWindow();
        window.ShowDialog();
        foreach(var action in window.AddedActions)
        {
            Actions.Add(action);
        }
    }
}
