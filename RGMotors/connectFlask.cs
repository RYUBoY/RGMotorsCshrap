using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using XGCommLib;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RGMotors
{
    public partial class MainWindow : Window
    {

        public class SunData
        {
            public int Id { get; set; }
            public int Coordinate { get; set; }
        }

        public class ApiService
        {
            private static readonly HttpClient client = new HttpClient();

            public SunData GetSunData()
            {
                string url = "http://127.0.0.1:5000/books2";  // Flask API 주소

                try
                {
                    // Flask API로부터 데이터 가져오기
                    HttpResponseMessage response = client.GetAsync(url).Result; // 동기적으로 결과 대기
                    response.EnsureSuccessStatusCode();

                    // 응답 내용을 JSON으로 파싱
                    string responseBody = response.Content.ReadAsStringAsync().Result; // 동기적으로 내용 읽기
                    SunData sunData = JsonConvert.DeserializeObject<SunData>(responseBody);

                    return sunData;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"요청 오류: {e.Message}");
                    return null;
                }
            }
        }

        private int FetchSunData()
        {
            SunData sunData = _apiService.GetSunData(); // 동기적으로 데이터 가져오기
            if (sunData != null)
            {
                // 데이터를 UI 요소에 반영
                string temp = $"Sun Coordinate: {sunData.Coordinate}";

                // 숫자 부분만 추출
                string numberPart = temp.Split(':')[1].Trim();

                // 문자열을 int로 변환
                int index = int.Parse(numberPart);
                return index;
            }
            else
            {
                MessageBox.Show("Failed to load data");
                return -1;
            }
        }

    }
}
