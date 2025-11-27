using System.Windows;
using UIPS.Client.Core.ViewModels;

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

            // 绑定上下文到viewModel
            var vm = new LoginViewModel();
            DataContext = vm;

            // 订阅 ViewModel 的成功事件
            vm.OnLoginSuccess += () =>
            {
                // 创建并显示主窗口
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // 关闭当前的登录窗口
                this.Close();
            };
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
