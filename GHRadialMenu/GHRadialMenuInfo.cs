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
    private static Keys _key;
    private static DateTime _currentDateTime;
    public static Keys LastKey
    {
        get => (DateTime.Now - _currentDateTime).TotalSeconds < 4 ? _key : Keys.None;
        set
        {
            _key = value;
            _currentDateTime = DateTime.Now;
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
        if (canvas.ActiveInteraction is RadioMenuInteraction) return true;
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