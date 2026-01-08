using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UIPS.Client.Services;
using UIPS.Client.ViewModels;

namespace UIPS.Client;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly UserSession _userSession;
    private bool _isLoaded = false;  // 标志：窗口是否已加载

    // 构造函数：由 DI 容器调用，并自动注入 IServiceProvider 和 UserSession
    public MainWindow(IServiceProvider serviceProvider, UserSession userSession)
    {
        _serviceProvider = serviceProvider;
        _userSession = userSession;
        
        // 必须先调用 InitializeComponent() 来创建 XAML 中定义的控件
        InitializeComponent();

        // 使用 Loaded 事件来确保窗口完全加载后再初始化
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 标记窗口已加载
        _isLoaded = true;

        // 现在可以安全地访问 XAML 控件了
        // 根据用户角色显示/隐藏管理员面板
        if (_userSession.IsAdmin)
        {
            AdminTab.Visibility = Visibility.Visible;
        }

        // 更新欢迎文本
        WelcomeText.Text = $"欢迎回来, {_userSession.UserName}";

        // 初始化导航
        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        // 防止在窗口加载之前访问控件
        if (!_isLoaded || ContentArea == null) return;

        // 从 DI 容器取出 DashboardView
        var view = _serviceProvider.GetRequiredService<Views.DashboardView>();

        // 从 DI 容器取出 ViewModel
        var viewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();

        // 绑定上下文
        view.DataContext = viewModel;

        // 放入主窗口的内容区域
        ContentArea.Content = view;
    }

    private void NavigateToAdmin()
    {
        // 防止在窗口加载之前访问控件
        if (!_isLoaded || ContentArea == null) return;

        // 从 DI 容器取出 AdminView
        var view = _serviceProvider.GetRequiredService<Views.AdminView>();

        // 从 DI 容器取出 ViewModel
        var viewModel = _serviceProvider.GetRequiredService<AdminViewModel>();

        // 绑定上下文
        view.DataContext = viewModel;

        // 放入主窗口的内容区域
        ContentArea.Content = view;
    }

    private void DashboardTab_Checked(object sender, RoutedEventArgs e)
    {
        NavigateToDashboard();
    }

    private void AdminTab_Checked(object sender, RoutedEventArgs e)
    {
        NavigateToAdmin();
    }

    /// <summary>
    /// 退出登录
    /// </summary>
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "确定要退出登录吗？",
            "退出确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        // 清除会话（包括本地存储的 Token）
        _userSession.ClearSession();

        // 打开登录窗口
        var loginView = _serviceProvider.GetRequiredService<Views.LoginView>();
        loginView.Show();

        // 关闭当前主窗口
        this.Close();
    }
}
