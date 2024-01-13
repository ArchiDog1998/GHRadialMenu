using Grasshopper;
using Grasshopper.GUI.Canvas;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace GHRadialMenu.Actions;
internal class NewObjectAction() : IAction
{
    public Guid ComponentGuid { get; set; }

    private string _name = string.Empty, _description = string.Empty;
    [JsonIgnore]
    public string Name => string.IsNullOrEmpty(_name) ? _name = Instances.ComponentServer.EmitObjectProxy(ComponentGuid).Desc.Name : _name;

    [JsonIgnore]
    public string Description => string.IsNullOrEmpty(_description) ? _description = Instances.ComponentServer.EmitObjectProxy(ComponentGuid).Desc.Description : _description;

    private Image? _icon = null;
    [JsonIgnore]
    public Image Icon => _icon ??= Instances.ComponentServer.EmitObjectProxy(ComponentGuid).Icon;

    public NewObjectAction(Guid componentGuid) : this()
    {
        ComponentGuid = componentGuid;
    }

    public void DoIt(GH_Canvas canvas, Point controlPoint)
    {
        PointF location = controlPoint;
        canvas.Viewport.Unproject(ref location);
        canvas.InstantiateNewObject(ComponentGuid, location, true);
    }
}