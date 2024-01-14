using GHRadialMenu.Actions;
using GHRadialMenu.Views.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ListBox = System.Windows.Controls.ListBox;

namespace GHRadialMenu.Views;
/// <summary>
/// Interaction logic for AddActionsWindow.xaml
/// </summary>
public partial class AddActionsWindow : Window
{
    public IAction[] AddedActions => ((AddActionsViewModel)DataContext).AddedActions;

    internal AddActionsViewModel ViewModel => (AddActionsViewModel)DataContext;

    public AddActionsWindow()
    {
        DataContext = new AddActionsViewModel(this);
        InitializeComponent();
        if (Data._editor != null) Owner = Data._editor;
    }

    private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        if (listBox.SelectedItem is not ToolStripMenuItem item) return;

        if (item.DropDownItems.Count == 0)
        {
            ViewModel.SelectedMenuItemCmdCommand.Execute(null);
        }
        else
        {
            ViewModel.Items.Add(item.Text);
            ViewModel.UpdateItems(item.DropDownItems);
        }
    }

    private void MenuItemSelectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedMenuItemCmdCommand.NotifyCanExecuteChanged();
    }
}
