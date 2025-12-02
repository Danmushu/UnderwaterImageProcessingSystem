using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UIPS.Client.Core.ViewModels;

namespace UIPS.Client;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    // 构造函数：由 DI 容器调用，并自动注入 IServiceProvider
    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        // 初始化导航
        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        // 从 DI 容器取出 DashboardView
        var view = _serviceProvider.GetRequiredService<Views.DashboardView>();

        // 从 DI 容器取出 ViewModel
        var viewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();

        // 绑定上下文
        view.DataContext = viewModel;

        // 放入主窗口的内容区域
        ContentArea.Content = view;
    }
}