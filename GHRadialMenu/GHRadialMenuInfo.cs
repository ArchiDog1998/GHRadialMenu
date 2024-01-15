using GHRadialMenu;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GHRadioMenu;

public class GHRadialMenuInfo : GH_AssemblyInfo
{
    public override string Name => "Radial Menu";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => Data.Icon;

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "The custom radial menu in Grasshopper!";

    public override Guid Id => new("32354c1e-486b-4508-88d9-85bcec332bc3");

    //Return a string identifying you or your company.
    public override string AuthorName => "秋水";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "1123993881@qq.com";
}

partial class SimpleAssemblyPriority : IDisposable
{
    private static Keys _lastkey;
    private static DateTime _lastKeyTime;
    public static Keys LastKey
    {
        get => (DateTime.Now - _lastKeyTime).TotalSeconds < 4 ? _lastkey : Keys.None;
        set
        {
            _lastkey = value;
            _lastKeyTime = DateTime.Now;
        }
    }

    private static Keys _maskkey;
    private static DateTime _maskKeyTime;
    public static Keys MaskKey
    {
        get => (DateTime.Now - _maskKeyTime).TotalSeconds < 0.5f ? _maskkey : Keys.None;
        set
        {
            _maskkey = value;
            _maskKeyTime = DateTime.Now;
        }
    }

    protected override int? MenuIndex => 1;
    protected override int InsertIndex => 16;


    protected override void DoWithEditor(GH_DocumentEditor editor)
    {
        CustomShortcutClicked += OnKeyDown;
        base.DoWithEditor(editor);
    }

    private bool OnKeyDown(Keys key)
    {
        var canvas = Instances.ActiveCanvas;
        if (canvas.ActiveInteraction is RadioMenuInteraction
            || MaskKey == key)
        {
            MaskKey = key;
            return true;
        }
        if (canvas.ActiveInteraction != null) return false;

        var shortcuts = Data.Shortcuts;

        foreach (var shortcut in shortcuts)
        {
            if (!shortcut.IsTriggered(LastKey, key)) continue;

            var actions = shortcut.Actions;
            var ctrlLoc = canvas.PointToClient(Cursor.Position);
            switch (actions.Length)
            {
                case 0:
                    continue;
                case 1:
                    actions[0].DoIt(canvas, ctrlLoc);
                    break;

                default:
                    PointF canvasLocation = ctrlLoc;
                    canvas.Viewport.Unproject(ref canvasLocation);

                    canvas.ActiveInteraction = new RadioMenuInteraction(ctrlLoc, canvasLocation, canvas, shortcut);
                    break;
            }

            MaskKey = key;
            LastKey = Keys.None;
            return true;
        }

        LastKey = key;
        return shortcuts.Any(s => s.FirstKey == key);
    }

    public void Dispose()
    {
        CustomShortcutClicked -= OnKeyDown;
        GC.SuppressFinalize(this);
    }
}