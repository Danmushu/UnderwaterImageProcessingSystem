using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
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
        private LoginViewModel? _viewModel;
        private bool _isUpdatingPassword = false;

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
            _viewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            DataContext = _viewModel;

            // 订阅登录成功事件
            _viewModel.OnLoginSuccess += NavigateToMainWindow;

            // 监听 ViewModel 的 Password 属性变化，同步到 PasswordBox
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        /// <summary>
        /// 监听 ViewModel 属性变化，同步密码到 PasswordBox
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isUpdatingPassword) return;

            if (e.PropertyName == nameof(LoginViewModel.Password) && _viewModel != null)
            {
                if (PasswordBox.Password != _viewModel.Password)
                {
                    _isUpdatingPassword = true;
                    PasswordBox.Password = _viewModel.Password;
                    _isUpdatingPassword = false;
                }
            }
            else if (e.PropertyName == nameof(LoginViewModel.ConfirmPassword) && _viewModel != null)
            {
                if (ConfirmPasswordBox.Password != _viewModel.ConfirmPassword)
                {
                    _isUpdatingPassword = true;
                    ConfirmPasswordBox.Password = _viewModel.ConfirmPassword;
                    _isUpdatingPassword = false;
                }
            }
        }

        /// <summary>
        /// 密码框内容变化时同步到 ViewModel
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword || _viewModel == null) return;

            _isUpdatingPassword = true;
            _viewModel.Password = PasswordBox.Password;
            _isUpdatingPassword = false;
        }

        /// <summary>
        /// 确认密码框内容变化时同步到 ViewModel
        /// </summary>
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword || _viewModel == null) return;

            _isUpdatingPassword = true;
            _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            _isUpdatingPassword = false;
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
