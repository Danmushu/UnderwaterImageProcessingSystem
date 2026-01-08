using System.Windows.Controls;
using UIPS.Client.ViewModels;

namespace UIPS.Client.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += DashboardView_Loaded;
    }

    private async void DashboardView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // 页面加载时自动加载图片列表
        if (DataContext is DashboardViewModel vm)
        {
            await vm.LoadImagesAsync();
        }
    }

    private void FileNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel && !string.IsNullOrEmpty(viewModel.SelectedFileName))
        {
            viewModel.SelectFileNameCommand.Execute(viewModel.SelectedFileName);
        }
    }
}