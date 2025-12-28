using System.Windows.Controls;
using Microsoft.Win32;
using UIPS.Client.ViewModels;

namespace UIPS.Client.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // 获取 DataContext (ViewModel)
        if (DataContext is DashboardViewModel vm)
        {
            // 创建并显示文件对话框
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp|所有文件|*.*",
                Title = "选择要上传的图片"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                //  将选中的文件路径设置回 ViewModel
                vm.SelectedFilePath = openFileDialog.FileName;
            }
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