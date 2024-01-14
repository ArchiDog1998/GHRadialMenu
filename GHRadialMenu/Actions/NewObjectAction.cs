using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using System;
using System.Drawing;

namespace GHRadialMenu.Actions;
internal class NewObjectAction() : IAction
{
    public Guid ComponentGuid { get; set; }

    private string _name = string.Empty, _description = string.Empty;
    [JsonIgnore]
    public string Name => string.IsNullOrEmpty(_name) ? _name = Proxy?.Desc.Name ?? _name : _name;

    [JsonIgnore]
    public string Description => string.IsNullOrEmpty(_description) ? _description = Proxy?.Desc.Description ?? _description : _description;

    private Image? _icon = null;
    [JsonIgnore]
    public Image? Icon => _icon ??= Proxy?.Icon;

    private IGH_ObjectProxy? _proxy;
    [JsonIgnore]
    public IGH_ObjectProxy? Proxy
    {
        get
        {
            if (_proxy != null) return _proxy;

            _proxy = Instances.ComponentServer.EmitObjectProxy(ComponentGuid);

            if (_proxy != null) return _proxy;

            foreach (var proxy in Instances.ComponentServer.ObjectProxies)
            {
                if (proxy.LibraryGuid != ComponentGuid) continue;

                return _proxy = proxy;
            }

            return null;
        }
    }

    public NewObjectAction(Guid componentGuid) : this()
    {
        ComponentGuid = componentGuid;
    }

    public void DoIt(GH_Canvas canvas, Point controlPoint)
    {
        PointF location = controlPoint;
        canvas.Viewport.Unproject(ref location);
        canvas.InstantiateNewObject(_proxy?.Guid ?? ComponentGuid, location, true);
    }
}