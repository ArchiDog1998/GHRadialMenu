using Grasshopper;
using Grasshopper.GUI.Canvas;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GHRadialMenu.Actions;
internal class MenuItemAction(params string[] itemHierarchy) : IAction
{
    public string[] ItemHierarchy { get; set; } = itemHierarchy;

    private string _name = string.Empty, _description = string.Empty;
    [JsonIgnore]
    public string Name => string.IsNullOrEmpty(_name) ? _name = TargetItem?.Text ?? _name : _name;

    [JsonIgnore]
    public string Description => string.IsNullOrEmpty(_description) ? _description = TargetItem?.ToolTipText ?? _description : _description;

    private Image? _icon = null;
    [JsonIgnore]
    public Image? Icon => _icon ??= TargetItem?.Image;

    private ToolStripMenuItem? _item = null;
    [JsonIgnore]
    public ToolStripMenuItem? TargetItem
    {
        get
        {
            if (_item != null) return _item;

            if (ItemHierarchy == null)
            {
                return null;
            }
            if (ItemHierarchy.Length < 2)
            {
                return null;
            }

            var menu = Instances.DocumentEditor.MainMenuStrip;
            if (menu == null) return null;

            ToolStripItemCollection collection = menu.Items;
            foreach (var name in ItemHierarchy)
            {
                _item = FindItem(collection, name);
                if (_item == null)
                {
                    return null;
                }

                collection = _item.DropDownItems;
            }

            return _item;
        }
    }

    private static ToolStripMenuItem? FindItem(ToolStripItemCollection items, string name)
    {
        var enumerator = items.GetEnumerator();

        try
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is ToolStripMenuItem item
                    && item.Text.Equals(name, StringComparison.Ordinal))
                {
                    return item;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        return null;
    }

    public void DoIt(GH_Canvas canvas, Point controlPoint)
    {
        TargetItem?.PerformClick();
    }
}
