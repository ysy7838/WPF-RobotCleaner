using System.Windows.Controls;
using System.Windows;
using WPF_Project.ViewModels;

namespace WPF_Project.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

    private void MapCell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button
            && DataContext is MapViewModel viewModel
            && viewModel.EditCellCommand.CanExecute(button.DataContext))
        {
            viewModel.EditCellCommand.Execute(button.DataContext);
        }
    }
}
