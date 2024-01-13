using Grasshopper.GUI.Canvas;
using System.Drawing;

namespace GHRadialMenu.Actions;
public interface IAction
{
    string Name { get; }
    string Description { get; }
    Image? Icon { get; }
    void DoIt(GH_Canvas canvas, Point controlPoint);
}
