﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GHRadialMenu.Actions;
using Grasshopper;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace GHRadialMenu.Views.ViewModels;
internal partial class AddActionsViewModel : ObservableObject
{
    private readonly AddActionsWindow _owner;

    public IAction[] AddedActions { get; private set; } = [];



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
        AddedActions = [.. Instances.ActiveDocument.SelectedObjects().Select(obj =>
        {
            var guid = obj.ComponentGuid;
            foreach (var proxy in Instances.ComponentServer.ObjectProxies)
            {
                if (proxy.Desc.Name != obj.Name) continue;
                if (proxy.Desc.SubCategory != obj.SubCategory) continue;
                if (proxy.Desc.Category != obj.Category) continue;
                if (proxy.GetType().FullName != "Grasshopper.Kernel.GH_UserObjectProxy") break;
                guid = proxy.LibraryGuid;
                break;
            }
            return new NewObjectAction(guid);
        })];
        _owner.Close();
    }

    private static bool HasDocument()
    {
        if (Instances.ActiveCanvas?.Document == null) return false;

        return Instances.ActiveDocument.SelectedObjects().Count > 0;
    }

    public void UpdateItems(ToolStripItemCollection? items)
    {
        MenuItems.Clear();

        if (items == null) return;
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

    [RelayCommand(CanExecute = nameof(HasSelectedItem))]
    public void SelectedMenuItemCmd()
    {
        if (_owner.MenuItemSelectList?.SelectedItem is not ToolStripMenuItem menu) return;
        AddedActions = [new MenuItemAction([.. Items, menu.Text])];
        _owner.Close();
    }

    public bool HasSelectedItem() => _owner.MenuItemSelectList?.SelectedItem is ToolStripMenuItem;
}
