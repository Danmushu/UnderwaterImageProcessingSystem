using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using UIPS.Client.ViewModels;

namespace UIPS.Client.Views
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            // 从 App 服务的容器中获取 ViewModel
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            var vm = serviceProvider.GetRequiredService<LoginViewModel>();

            DataContext = vm;

            // 订阅 ViewModel 的成功事件
            vm.OnLoginSuccess += () =>
            {
                // 确保使用 serviceProvider 来获取 MainWindow
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>(); // <-- 修复行
                mainWindow.Show();

                this.Close();
            };
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
