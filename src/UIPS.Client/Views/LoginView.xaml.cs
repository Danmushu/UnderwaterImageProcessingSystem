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

            // 绑定数据上下文到 ViewModel
             DataContext = new LoginViewModel();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
