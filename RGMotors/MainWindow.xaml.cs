using System;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
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
using System.Windows.Threading;
using XGCommLib;
using static RGMotors.MainWindow;

namespace RGMotors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private DispatcherTimer timer;
        private bool isVideoFinished = false; // 비디오 종료 상태 추적 변수
        private int x1Value;

        public MainWindow()
        {
            InitializeComponent();
        }
        CommObject20 oCommDriver = null;
        CommObjectFactory20 factory = new CommObjectFactory20();
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
            
            PanelButton.Visibility = Visibility.Visible;
        }

        private void PanelButton_click(object sender, RoutedEventArgs e)
        {
           
        }

        private async void getSunData_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.GetStringAsync("http://127.0.0.1:5000/reset_x1");
            StartSunTracking();
            StartTimer();
        }

        private async void StartSunTracking()
        {
            try
            {
                await client.GetStringAsync("http://127.0.0.1:5000/books1");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting sun tracking: {ex.Message}");
            }
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 1초마다 호출
            timer.Tick += async (sender, e) => await FetchX1Value();
            timer.Start();
        }

        private async Task FetchX1Value()
        {

            try
            {
                var response = await client.GetStringAsync("http://127.0.0.1:5000/x1");
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                x1Value = json.x1;

                // 비디오가 종료된 경우 처리
                if (x1Value == -1 && !isVideoFinished) // 종료 상태를 나타내는 값
                {
                    isVideoFinished = true; // 종료 상태로 설정
                    timer.Stop(); // 타이머 중지
                    MessageBox.Show("비디오가 종료되었습니다.");
                    return;
                }
                else if (x1Value != -1)
                {
                    isVideoFinished = false; // 비디오가 다시 재생 중인 경우
                }

                // UI 업데이트를 안전하게 수행
                Dispatcher.Invoke(() =>
                {
                    //TextBox_Result.Text = $"Current x1 value: {x1Value}";
                    oCommDriver = connectPLC(oCommDriver, factory);
                    showPanel(oCommDriver, factory, x1Value);
                });

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching x1 value: {ex.Message}");
            }
        }
    }
}