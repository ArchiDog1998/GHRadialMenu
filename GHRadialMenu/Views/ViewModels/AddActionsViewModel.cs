using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GHRadialMenu.Actions;
using Grasshopper;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GHRadialMenu.Views.ViewModels;
internal partial class AddActionsViewModel : ObservableObject
{
    private readonly AddActionsWindow _owner;

    public IAction[] AddedActions { get; private set; } = [];

    private static bool HasDocument => Instances.ActiveCanvas?.IsDocument ?? false;


    [ObservableProperty]
    private ObservableCollection<string> _items = [];

    [ObservableProperty]
    private ObservableCollection<ToolStripMenuItem> _menuItems = [];

    public AddActionsViewModel(AddActionsWindow owner)
    {
        _owner = owner;
        UpdateItems(Instances.DocumentEditor.MainMenuStrip?.Items);
    }

    [RelayCommand(CanExecute = nameof(HasDocument))]
    public void SelectedObjectsCmd()
    {
        AddedActions = [..Instances.ActiveDocument.SelectedObjects().Select(obj =>
        {
            var guid = obj.ComponentGuid;
            if (obj is GH_Cluster || obj.GetType().FullName
                is "GhPython.Component.PythonComponent_OBSOLETE"
                or "GhPython.Component.ZuiPythonComponent"
                or "ScriptComponents.Component_CSNET_Script"
                or "ScriptComponents.Component_CSNET_Script_OBSOLETE"
                or "ScriptComponents.Component_VBNET_Script"
                or "ScriptComponents.Component_VBNET_Script_OBSOLETE"
                or "RhinoCodePluginGH.Components.ScriptComponent")
            {
                foreach (var proxy in Instances.ComponentServer.ObjectProxies)
                {
                    if (proxy.Desc.Name != obj.Name) continue;
                    if (proxy.Desc.SubCategory != obj.SubCategory) continue;
                    if (proxy.Desc.Category != obj.Category) continue;
                    guid = proxy.LibraryGuid;
                    break;
                }
            }
            return new NewObjectAction(guid);
        })];
        _owner.Close();
    }

    public void UpdateItems(ToolStripItemCollection? items)
    {
        MenuItems.Clear();

        if(items == null) return;
        var enumerator = items.GetEnumerator();

        try
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not ToolStripMenuItem item) continue;
                MenuItems.Add(item);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    [RelayCommand(CanExecute =nameof(HasSelectedItem))]
    public void SelectedMenuItemCmd()
    {
        if (_owner.MenuItemSelectList?.SelectedItem is not ToolStripMenuItem menu) return;
        AddedActions = [new MenuItemAction([.. Items, menu.Text])];
        _owner.Close();
    }

    public bool HasSelectedItem() => _owner.MenuItemSelectList?.SelectedItem is ToolStripMenuItem;
}
