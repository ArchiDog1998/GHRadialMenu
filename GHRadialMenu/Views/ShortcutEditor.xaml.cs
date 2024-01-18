using GHRadialMenu.Actions;
using GHRadialMenu.Views.ViewModels;
using SimpleGrasshopper.Util;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GHRadialMenu.Views;
/// <summary>
/// Interaction logic for ShortcutEditor.xaml
/// </summary>
public partial class ShortcutEditor : Window
{
    public ShortcutEditor()
    {
        DataContext = new ShortcutEditorViewModel();
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Data._editor = null;
    }

    private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key;

        if (key is System.Windows.Input.Key.LeftCtrl
            or System.Windows.Input.Key.RightCtrl
            or System.Windows.Input.Key.LeftAlt
            or System.Windows.Input.Key.RightAlt
            or System.Windows.Input.Key.LWin
            or System.Windows.Input.Key.RWin
            or System.Windows.Input.Key.LeftShift
            or System.Windows.Input.Key.RightShift)
        {
            return;
        }

        if (e.OriginalSource is not DataGridCell cell) return;
        if (cell.Column is not DataGridBoundColumn column) return;
        if (cell.DataContext is not ShortcutViewModel data) return;

        var propertyName = ((Binding)column.Binding)?.Path.Path;

        var property = typeof(ShortcutViewModel).GetAllRuntimeProperties().First(p => p.Name == propertyName);
        if (property == null || property.PropertyType != typeof(System.Windows.Forms.Keys)) return;


        var Key = key == System.Windows.Input.Key.Escape ? System.Windows.Forms.Keys.None : System.Windows.Forms.Control.ModifierKeys
            | (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);

        property.SetValue(data, Key);
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        if (listBox.DataContext is not ShortcutViewModel data) return;

        data.Selected = listBox.SelectedIndex;
    }
}

[ValueConversion(typeof(System.Drawing.Image), typeof(BitmapImage))]
public class BitmapConverter : IValueConverter
{
    public static BitmapImage ToImageSource(System.Drawing.Image bitmap)
    {
        MemoryStream ms = new MemoryStream();
        bitmap?.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        BitmapImage image = new BitmapImage();

        image.BeginInit();
        ms.Seek(0, SeekOrigin.Begin);
        image.StreamSource = ms;
        image.EndInit();
        return image;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null!;

        System.Drawing.Bitmap picture = (System.Drawing.Bitmap)value;
        return ToImageSource(picture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null!;
    }
}


[ValueConversion(typeof(IAction), typeof(string))]
public class TooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null!;

        var act = (IAction)value;

        return $"{act.Name}\n{act.Description}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null!;
    }
}