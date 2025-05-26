// LoginWindow.xaml.cs

using System.Windows;

namespace WPF_UI_UX // Đảm bảo namespace này khớp với project của bạn
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text; 
            string password = txtPassword.Password; 

            if (username == "admin" && password == "123") 
            {
               

                this.DialogResult = true;
              
                this.Close(); 
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; 
            this.Close();
        }
    }
}