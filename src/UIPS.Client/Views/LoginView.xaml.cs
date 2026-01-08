using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UIPS.Client.Services;
using UIPS.Client.ViewModels;

namespace UIPS.Client.Views
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public LoginView()
        {
            InitializeComponent();

            _serviceProvider = ((App)Application.Current).ServiceProvider;

            // 检查是否已有保存的会话（自动登录）
            var userSession = _serviceProvider.GetRequiredService<UserSession>();
            if (userSession.IsAuthenticated)
            {
                // 已有有效会话，直接跳转到主界面
                Loaded += (s, e) => NavigateToMainWindow();
                return;
            }

            // 未登录，显示登录界面
            var vm = _serviceProvider.GetRequiredService<LoginViewModel>();
            DataContext = vm;

            // 订阅登录成功事件
            vm.OnLoginSuccess += NavigateToMainWindow;
        }

        /// <summary>
        /// 跳转到主界面
        /// </summary>
        private void NavigateToMainWindow()
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
