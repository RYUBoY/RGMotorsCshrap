using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XGCommLib;

namespace RGMotors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)//애플리케이션 종료 버튼
        {
            Application.Current.Shutdown(); // WPF 애플리케이션 종료
        }

        //관리자 모드

        private void managerLoginButton_Click(object sender, RoutedEventArgs e)//관리자 모드 로그인 버튼
        {
            login();
        }

        private string adminID = "admin123";//관리자 아이디
        private string adminPW = "admin123";//관리자 비밀번호
        public void login() //로그인 로직
        {
            if (idTextBox.Text == adminID)
            {
                if (pwTextBox.Password == adminPW)
                {
                    MessageBox.Show("로그인 완료!");
                    loginGrid.Visibility = Visibility.Collapsed;
                    managerPage.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("잘못된 비밀번호입니다.");
                }
            }
            else
            {
                MessageBox.Show("잘못된 아이디입니다.");
            }
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)//GitHub 버튼
        {
            string url = "https://github.com/KimJongHoss/RGMotorsCshrap";  // 여기에 원하는 URL을 입력하세요
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true  // 이 옵션은 .NET Core와 .NET Framework에서 URL을 열 때 필요합니다
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open link: " + ex.Message);
            }
        }

        private void testButton_Click(object sender, RoutedEventArgs e)//테스트 버튼 클릭시
        {
            test();
        }

        private void connectPLC_Click(object sender, RoutedEventArgs e)
        {
            CommObject20 oCommDriver = null;
            oCommDriver = connectPLC();
        }
    }
}