using GHRadialMenu.Actions;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using Grasshopper.Kernel;
using Rhino;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GHRadialMenu;
internal class RadioMenuInteraction : IGH_MouseInteraction
{
    private const int CircleRadius = 15, MinInnerRadius = 50;

    public Point ControlPointDown { get; }
    public PointF CanvasPointDown { get; }
    public GH_Canvas Owner { get; }
    public IAction[] Actions { get; }
    public Rectangle[] Rectangles { get; }
    public string Name { get; }
    public int Index { get; private set; } = -1;
    public int InnerRadius { get; }

    public bool IsActive => true;
    public bool DeactivateOnFocusLoss => false;
    public bool TooltipEnabled => true;
    public static Point ControlLocation => Instances.ActiveCanvas.PointToClient(Cursor.Position);

    public RadioMenuInteraction(Point ctrlLoc, PointF canvasLoc, GH_Canvas canvas, Shortcut shortcut)
    {
        ControlPointDown = ctrlLoc;
        CanvasPointDown = canvasLoc;
        Owner = canvas;
        Actions = shortcut.Actions;
        Name = shortcut.Name;
        Instances.ActiveCanvas.CanvasPostPaintWidgets += Render;

        var count = Actions.Length;
        InnerRadius = (int)Math.Max(count * 30 / Math.Tau, MinInnerRadius);
        var radius = InnerRadius + 20;

        Rectangles = new Rectangle[count];
        for (int i = 0; i < count; i++)
        {
            var alpha = MathF.Tau * i / count;
            var x1 = ControlPointDown.X + radius * MathF.Cos(alpha);
            var y1 = ControlPointDown.Y - radius * MathF.Sin(alpha);

            var action = Actions[i];
            var width = action.Icon == null ? GH_FontServer.StringWidth(action.Name, GH_FontServer.StandardBold) / 2 : 12;
            Rectangles[i] = CenterRectangle(new Point((int)x1, (int)y1), width, 12);
        }
    }

    private void Render(GH_Canvas sender)
    {
        var graphics = sender.Graphics;
        var transform = graphics.Transform;
        graphics.ResetTransform();
        graphics.SmoothingMode = SmoothingMode.HighQuality;

        var count = Actions.Length;
        var outerRadius = InnerRadius + 50;
        var ctrlLoc = ControlLocation;
        var angle = (float)RhinoMath.ToDegrees(MathF.Atan2(ctrlLoc.Y - ControlPointDown.Y, ctrlLoc.X - ControlPointDown.X));

        UpdateIndex();
        DrawCircle();
        DrawName();
        DrawBackground();
        DrawSpokes();
        DrawIcons();

        graphics.Transform = transform;

        void UpdateIndex()
        {
            if (Math.Pow(ctrlLoc.X - ControlPointDown.X, 2) + Math.Pow(ctrlLoc.Y - ControlPointDown.Y, 2) < CircleRadius * CircleRadius)
            {
                Index = -1;
            }
            else
            {
                var span = MathF.Tau / count;
                var calculatedRadian = RhinoMath.ToRadians(-angle) + span / 2;
                Index = (int)Math.Floor((calculatedRadian + MathF.Tau) % MathF.Tau / span);
            }
        }

        void DrawCircle()
        {
            Rectangle rectangle = Rectangle.FromLTRB(ControlPointDown.X - CircleRadius, ControlPointDown.Y - CircleRadius,
                ControlPointDown.X + CircleRadius, ControlPointDown.Y + CircleRadius);

            using var pen = new Pen(Color.FromArgb(100, Color.Black), 7);
            graphics.DrawEllipse(pen, rectangle);
            pen.Color = Color.FromArgb(128, 145, 200, 240);

            if (Index < 0) return;

            graphics.DrawArc(pen, rectangle, angle - 20, 40);
        }

        void DrawName()
        {
            using var brush = new SolidBrush(Color.FromArgb(180, Color.Black));
            graphics.DrawString(Name, GH_FontServer.StandardBold, brush, new PointF(ControlPointDown.X, ControlPointDown.Y - 30), GH_TextRenderingConstants.CenterCenter);
        }

        void DrawBackground()
        {
            var rectangle = CenterRectangle(InnerRadius);

            using (var brush = WheelBrush(Color.FromArgb(255, 50, 50, 50), Color.FromArgb(0, 50, 50, 50), outerRadius))
            {
                using var graphicsPath = new GraphicsPath();

                graphicsPath.AddEllipse(rectangle);
                graphicsPath.AddEllipse(CenterRectangle(outerRadius));
                graphics.FillPath(brush, graphicsPath);
            }

            using (var linearGradientBrush = new LinearGradientBrush(rectangle, Color.FromArgb(150, Color.White), Color.FromArgb(0, Color.White), LinearGradientMode.Vertical)
            {
                WrapMode = WrapMode.TileFlipXY,
            })
            {
                using var pen = new Pen(linearGradientBrush, 2f);
                Rectangle rect = rectangle;
                rect.Inflate(1, 1);
                graphics.DrawEllipse(pen, rect);
            }

            using var pen2 = new Pen(Color.FromArgb(200, Color.Black), 1f);
            graphics.DrawEllipse(pen2, rectangle);
        }

        void DrawSpokes()
        {
            using var brush = WheelBrush(Color.FromArgb(80, Color.Black), Color.FromArgb(0, Color.Black), 150);
            using var pen = new Pen(brush, 1f)
            {
                DashPattern = [2f, 2f],
            };

            for (var i = 0.5f; i < count; i++)
            {
                var alpha = MathF.Tau * i / count;
                var x1 = ControlPointDown.X + InnerRadius * MathF.Cos(alpha);
                var y1 = ControlPointDown.Y - InnerRadius * MathF.Sin(alpha);
                var x2 = ControlPointDown.X + outerRadius * MathF.Cos(alpha);
                var y2 = ControlPointDown.Y - outerRadius * MathF.Sin(alpha);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        void DrawIcons()
        {
            using var brush = new SolidBrush(Color.FromArgb(180, Color.Black));

            for (int i = 0; i < count; i++)
            {
                var rect = Rectangles[i];
                if (Index == i)
                {
                    Rectangle box = rect;
                    box.Inflate(2, 2);
                    GH_GraphicsUtil.RenderHighlightBox(graphics, box, 2);
                }

                var icon = Actions[i].Icon;
                if (icon != null)
                {
                    GH_GraphicsUtil.RenderIcon(graphics, rect, icon);
                }
                else
                {
                    graphics.DrawString(Actions[i].Name, GH_FontServer.StandardBold, brush,
                        rect.Location + rect.Size / 2, GH_TextRenderingConstants.CenterCenter);
                }
            }
        }
    }

    private Brush WheelBrush(Color inner, Color outer, int radius)
    {
        var graphicsPath = new GraphicsPath();
        graphicsPath.AddEllipse(CenterRectangle(radius));

        return new PathGradientBrush(graphicsPath)
        {
            SurroundColors = [outer],
            CenterColor = inner,
            CenterPoint = ControlPointDown,
        };
    }

    private Rectangle CenterRectangle(int radius)
    {
        return CenterRectangle(ControlPointDown, radius, radius);
    }

    private static Rectangle CenterRectangle(Point point, int width, int height)
    {
        return Rectangle.FromLTRB(point.X - width, point.Y - height, point.X + width, point.Y + height);
    }

    public void Destroy()
    {
        Instances.ActiveCanvas.CanvasPostPaintWidgets -= Render;
    }

    public GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        sender.Refresh();
        return GH_ObjectResponse.Ignore;
    }

    public GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        return GH_ObjectResponse.Ignore;
    }

    public GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (e.Button != MouseButtons.Right)
        {
            PerformAction();
        }
        return GH_ObjectResponse.Release;
    }

    public GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        return GH_ObjectResponse.Ignore;
    }

    public GH_ObjectResponse RespondToKeyDown(GH_Canvas sender, KeyEventArgs e)
    {
        return GH_ObjectResponse.Ignore;
    }

    public GH_ObjectResponse RespondToKeyUp(GH_Canvas sender, KeyEventArgs e)
    {
        PerformAction();
        return GH_ObjectResponse.Release;
    }

    private void PerformAction()
    {
        if (Index < 0) return;
        Actions[Index].DoIt(Owner, ControlPointDown);
    }

    public bool IsTooltipRegion(PointF canvasPoint)
    {
        Owner.Viewport.Project(ref canvasPoint);
        var ctrlPt = new Point((int)canvasPoint.X, (int)canvasPoint.Y);
        return Rectangles.Any(rect => rect.Contains(ctrlPt));
    }

    public void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
    {
        Owner.Viewport.Project(ref canvasPoint);
        var ctrlPt = new Point((int)canvasPoint.X, (int)canvasPoint.Y);
        for (int i = 0; i < Actions.Length; i++)
        {
            var rect = Rectangles[i];
            if (!rect.Contains(ctrlPt)) continue;

            var action = Actions[i];
            e.Title = action.Name;
            e.Description = action.Description;
            e.Icon = (Bitmap)action.Icon!;
        }
    }
}
