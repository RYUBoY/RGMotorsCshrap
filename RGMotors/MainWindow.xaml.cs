using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;
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
        int TempL = 0;
        int TempS = 0;
        int Lcnt = 0;
        int Scnt = 0;
        public ChartValues<int> Values { get; set; } = new ChartValues<int>();

        private readonly Dictionary<int, int> x1ValueToOutput = new Dictionary<int, int>

        {
            { 1, 50 },
            { 2, 70 },
            { 4, 100 },
            { 32, 70 },
            { 64, 50 }
        };


        public MainWindow()
        {
            InitializeComponent();
            StartHttpListener(); // WPF 내에서 HTTP 서버 시작

        }
        CommObject20 oCommDriver = null;
        CommObjectFactory20 factory = new CommObjectFactory20();
        private void buttonExit_Click(object sender, RoutedEventArgs e)//애플리케이션 종료 버튼
        {
            Application.Current.Shutdown(); // WPF 애플리케이션 종료
        }
        private async Task GetBooksAsync()
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync("http://127.0.0.1:5000/books");

                    // 응답 데이터를 JSON으로 파싱
                    JArray booksArray = JArray.Parse(response);

                    // 처음에 한 번만 텍스트를 설정
                    LBox.Text = "누적 " + booksArray[0]["count"].ToString() + " 개";
                    SBox.Text = "누적 " + booksArray[1]["count"].ToString() + " 개";

                    // L size
                    if (int.Parse(booksArray[0]["count"].ToString()) > TempL)
                    {
                        string size = booksArray[0]["size"].ToString();
                        TempL++;
                        Lcnt++;

                        // Large 박스 생성 및 이동
                        Image newLargeBox = CreateBox("large");
                        MoveBox(newLargeBox, originalXSmallbox); // 새로운 박스 이동

                        // 증가된 카운트는 따로 표시 (필요하다면 다른 텍스트 박스 사용)
                        // Lcnt의 값을 다른 박스에 출력하거나 로그로 남기는 식으로 처리 가능
                    }

                    // S size
                    else if (int.Parse(booksArray[1]["count"].ToString()) > TempS)
                    {
                        string size = booksArray[1]["size"].ToString();
                        TempS++;
                        Scnt++;

                        // Small 박스 생성 및 이동
                        Image newSmallBox = CreateBox("small");
                        MoveBox(newSmallBox, originalXLargelbox); // 새로운 박스 이동

                        // 증가된 카운트도 따로 처리
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"GET 요청 실패: {ex.Message}");
                }
            }
        }

        // WPF에서 HttpListener를 사용하여 Flask로부터 알림을 받는 메소드
        private async void StartHttpListener()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/"); // Flask가 이 주소로 요청 보냄
            listener.Start();

            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;

                if (request.HttpMethod == "POST")
                {
                    // 요청을 받은 후 GET 메소드를 실행
                    await GetBooksAsync();
                }

                var response = context.Response;
                string responseString = "<html><body>OK</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }

        private void ChangeSpeed()
        {
            gage.Text = conveyorspeed.Value.ToString("0");
            double speedFactor = 1 + (conveyorspeed.Value / 100.0) * 2; // 0 -> 1배속, 100 -> 2배속
            SetVideoSpeed(speedFactor);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //gage.Text = sd.Value.ToString("0");

            // 슬라이더 값을 Flask 서버로 보내어 영상 재생 속도를 조절
            double speedFactor = 1 + (sd.Value / 100.0); // 0 -> 1배속, 100 -> 2배속
            SetVideoSpeed(speedFactor);
        }

        private async void SetVideoSpeed(double speedFactor)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var json = new JObject { { "speed", speedFactor } };
                var content = new System.Net.Http.StringContent(json.ToString(), System.Text.Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("http://127.0.0.1:5000/set_speed", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"속도 설정 실패: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"속도 설정 중 오류 발생: {ex.Message}");
                }
            }
        }

        private async void StartVideoAsync()
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await client.PostAsync("http://127.0.0.1:5000/video/start", null);
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"영상 시작 실패: {response.StatusCode}");
                    }
                    else
                    {
                        //  MessageBox.Show("영상이 시작되었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"영상 시작 중 오류 발생: {ex.Message}");
                }
            }
        }

        private async void StopVideoAsync()
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await client.PostAsync("http://127.0.0.1:5000/video/stop", null);
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"영상 일시정지 실패: {response.StatusCode}");
                    }
                    else
                    {
                        //    MessageBox.Show("영상이 일시정지되었습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"영상 일시정지 중 오류 발생: {ex.Message}");
                }
            }
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            StartVideoAsync();  // Flask 서버로 영상 시작 신호 전송

            // 모든 애니메이션을 재개
            ResumeAllBoxes();

            // 박스의 상태를 업데이트
            UpdateBoxes();

            var controller = ImageBehavior.GetAnimationController(this.ConveyorBelt1);
            var controller2 = ImageBehavior.GetAnimationController(this.ConveyorBelt2);

            controller.Play();
            controller2.Play();
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            StopVideoAsync();  // Flask 서버로 영상 일시정지 신호 전송

            // 모든 애니메이션을 일시 정지
            PauseAllBoxes();

            var controller = ImageBehavior.GetAnimationController(this.ConveyorBelt1);
            var controller2 = ImageBehavior.GetAnimationController(this.ConveyorBelt2);

            controller.Pause();
            controller2.Pause();
        }


        private bool isLargeBoxLoaded = false; // largebox가 로드되었는지 상태
        private bool isSmallBoxLoaded = false; // smallbox가 로드되었는지 상태
                                               // 관리자 모드
        private Image CreateBox(string boxType)
        {
            Image newBox = new Image();
            if (boxType == "large")
            {
                newBox.Source = new BitmapImage(new Uri("Resources/largebox.png", UriKind.Relative));
                Canvas.SetLeft(newBox, originalXLargelbox);
                Canvas.SetTop(newBox, 20);
                isLargeBoxLoaded = true; // largebox 상태를 로드됨으로 설정
            }
            else if (boxType == "small")
            {
                newBox.Source = new BitmapImage(new Uri("Resources/smallbox.png", UriKind.Relative));
                Canvas.SetLeft(newBox, originalXSmallbox);
                Canvas.SetTop(newBox, 297);
                isSmallBoxLoaded = true; // smallbox 상태를 로드됨으로 설정
            }
            newBox.Width = 203;
            newBox.Height = 195;
            newBox.Visibility = Visibility.Visible;

            myCanvas.Children.Add(newBox); // myCanvas는 <Canvas>의 x:Name

            // 박스를 변수에 저장
            if (boxType == "large") largebox = newBox;
            else if (boxType == "small") smallbox = newBox;

            return newBox;
        }

        private double originalXSmallbox = 250;
        private double originalXLargelbox = 270;

        private List<Storyboard> activeStoryboards = new List<Storyboard>(); // 활성화된 Storyboard 목록
        private Storyboard currentStoryboard;

        private void MoveBox(Image box, double originalX)
        {
            double targetY = (box.Source.ToString().Contains("largebox")) ? 20 : 297; // largebox는 높이 10, smallbox는 높이 280
            Canvas.SetTop(box, targetY); // 박스의 Y 위치 설정
            Canvas.SetLeft(box, originalX); // 박스의 X 위치 설정

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                From = originalX,  // 원래 위치에서 시작
                To = originalX + 420,  // 420만큼 이동
                Duration = new Duration(TimeSpan.FromSeconds(2)), // 2초 동안 이동
                FillBehavior = FillBehavior.HoldEnd // 애니메이션 종료 후 마지막 상태 유지
            };

            // 애니메이션이 끝나면 박스를 숨김
            moveAnimation.Completed += (s, e) =>
            {
                box.Visibility = Visibility.Hidden;
            };

            // Storyboard 설정
            Storyboard boxStoryboard = new Storyboard(); // 개별 애니메이션을 위한 스토리보드
            boxStoryboard.Children.Add(moveAnimation);
            Storyboard.SetTarget(moveAnimation, box);
            Storyboard.SetTargetProperty(moveAnimation, new PropertyPath("(Canvas.Left)"));

            // 애니메이션 시작
            boxStoryboard.Begin();

            // 활성화된 Storyboard 목록에 추가
            activeStoryboards.Add(boxStoryboard);
        }

        private void PauseAllBoxes()
        {
            foreach (var storyboard in activeStoryboards)
            {
                storyboard.Pause(); // 모든 Storyboard를 멈춤
            }
        }
        private void ResumeAllBoxes()
        {
            foreach (var storyboard in activeStoryboards)
            {
                storyboard.Resume(); // 모든 Storyboard를 재개
            }
        }

        private void UpdateBoxes()
        {
            // Lcnt가 증가하면 largebox 이동
            if (Lcnt > 0 && !largebox.IsLoaded)
            {
                // 박스가 이미 로드된 상태가 아니면 이동 시작
                largebox.Visibility = Visibility.Visible; // 박스를 보이게 하고
                MoveBox(largebox, originalXSmallbox);
            }

            // Scnt가 증가하면 smallbox 이동
            if (Scnt > 0 && !smallbox.IsLoaded)
            {
                // 박스가 이미 로드된 상태가 아니면 이동 시작
                smallbox.Visibility = Visibility.Visible; // 박스를 보이게 하고
                MoveBox(smallbox, originalXLargelbox);
            }
        }


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

        private async void getSunData_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.GetStringAsync("http://127.0.0.1:5000/reset_x1");
            StartSunTracking();
            StartTimer();
            this.DataContext = this; // 차트에 바인딩할 데이터를 설정
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            ChangeSpeed(); // 1초마다 ChangeSpeed() 호출
        }
        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 1초마다 호출
            timer.Tick += async (sender, e) => await FetchX1Value();
            timer.Tick += Timer_Tick; // Tick 이벤트 핸들러 등록
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

                // x1Value에 해당하는 출력 값 찾기 (차트에 추가할 값만 매핑)
                int outputValue = 0;
                if (x1ValueToOutput.ContainsKey(x1Value))
                {
                    outputValue = x1ValueToOutput[x1Value];
                }
                else
                {
                    // 매핑되지 않은 값은 기본값을 사용 (예: 0)
                    outputValue = 0;
                }
                // UI 업데이트를 안전하게 수행
                Dispatcher.Invoke(() =>
                {
                    //TextBox_Result.Text = $"Current x1 value: {x1Value}";
                    oCommDriver = connectPLC(oCommDriver, factory);
                    showPanel(oCommDriver, factory, x1Value);
                    Values.Add(outputValue); // 새로운 값 추가
                    if (Values.Count > 10) // 그래프가 너무 길어지지 않도록 제한
                    {
                        Values.RemoveAt(0); // 처음 값을 제거하여 차트가 최신 100개의 값만 보이도록 설정
                    }
                });


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching x1 value: {ex.Message}");
            }
        }

    }
}