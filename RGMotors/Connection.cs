using System;
using System.Windows;
using System.Windows.Controls;
using XGCommLib;

namespace RGMotors
{
    public partial class MainWindow : Window
    {
        public void test()
        {
            MessageBox.Show("Connection Class is working");
        }



        public CommObject20 connectPLC(CommObject20 oCommDriver, CommObjectFactory20 factory)
        {
            try
            {
                oCommDriver = factory.GetMLDPCommObject20("192.168.1.201:2004");

                if (oCommDriver == null)
                {
                    MessageBox.Show("oCommDriver가 null입니다. 연결 실패.");
                }

                //MessageBox.Show("oCommDriver 생성됨");
                

                int connectResult = oCommDriver.Connect("");
                if (connectResult == 0)
                {
                    
                    return null;
                }
                else
                {
                    //MessageBox.Show("connect 성공!");
                    return oCommDriver;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생: " + ex.Message);
                return null;
            }
        }

        public void showPanel(CommObject20 oCommDriver, CommObjectFactory20 factory20, int index)
        {


            int nMaxBuf = 1400;
            byte[] bufWrite = new byte[nMaxBuf];

            int nTotal_len = 0;

            XGCommLib.DeviceInfo oDevice = factory20.CreateDevice();
            oDevice.ucDataType = (byte)'B';
            oDevice.ucDeviceType = (byte)'M';


            oDevice.lOffset = 0;
            oDevice.lSize = 1;


            oCommDriver.AddDeviceInfo(oDevice);

            bufWrite[0] = (byte)x1Value;
            nTotal_len += 1;

            byte[] bWriteBuf = new byte[nTotal_len];
            Array.Copy(bufWrite, 0, bWriteBuf, 0, nTotal_len);

            if (1 == oCommDriver.WriteRandomDevice(bWriteBuf))
            {
                //MessageBox.Show("bWriteBuf success");
            }
            else
            {
                MessageBox.Show("bWriteBuf fail");
            }

            byte[] bufRead = new byte[nTotal_len];
            if (1 == oCommDriver.ReadRandomDevice(bufRead))
            {
                int offset = (int)oDevice.lOffset;

                if (offset >= 0 && offset < bufRead.Length)
                {
                    byte resultValue = bufRead[offset];

                    UpdateGauge(resultValue); // 라디알 가그 업데이트

                    byte mask1 = 0b00000001; // 1
                    byte mask2 = 0b00000010; // 2
                    byte mask3 = 0b00000100; // 4
                    byte mask4 = 0b00100000; // 32
                    byte mask5 = 0b01000000; // 64

                    // masks 배열에 mask 값을 저장
                    int[] masks = { mask1, mask2, mask3, mask4, mask5 };
                    bool[] results = new bool[masks.Length];  // 결과를 저장할 배열

                    // 반복문을 사용하여 비트 연산 수행
                    for (int i = 0; i < masks.Length; i++)
                    {
                        results[i] = (resultValue & masks[i]) != 0;
                    }

                    //// results 배열에 각 mask에 대한 결과가 저장됨
                    //bool isOne = results[0];
                    //bool isTwo = results[1];
                    //bool isThree = results[2];
                    //bool isFour = results[3];
                    //bool isFive = results[4];

                    // 패널들을 배열로 정리
                    UIElement[] solarPanels = { solarpanel1, solarpanel2, solarpanel3, solarpanel4, solarpanel5 };

                    // 모든 패널의 Visibility를 Collapsed로 초기화
                    foreach (var panel in solarPanels)
                    {
                        panel.Visibility = Visibility.Collapsed;
                    }



                    // switch문을 사용한 선택 로직


                    switch (index)
                    {
                        case 1:
                            index = 0;
                            break;
                        case 2:
                            index = 1;
                            break;
                        case 4:
                            index = 2;
                            break;
                        case 32:
                            index = 3;
                            break;
                        case 64:
                            index = 4;
                            break;
                    }




                    if (index >= 0)
                    {
                        solarPanels[index].Visibility = Visibility.Visible;
                    }

                    // 결과 출력
                    for (int i = 0; i < bufRead.Length; i++)
                    {
                        TextBox_Result.AppendText(bufRead[i].ToString("X2"));
                    }
                }
            }
            else
            {
                MessageBox.Show("읽기 실패!");
            }

            if (!bWriteBuf.SequenceEqual(bufRead))
            {
                TextBox_Result.AppendText("Mismatch");
            }
            else
            {
                TextBox_Result.AppendText("Match");
            }

            int nRetn = oCommDriver.Disconnect();
            if (nRetn == 1)
            {
                //MessageBox.Show("Disconnect Success");
            }
            else
            {
                MessageBox.Show("Disconnect fail");
            }
        }

        private void UpdateGauge(byte resultValue) // 라디알 가그
        {
            byte mask1 = 0b00000001; // 1
            byte mask2 = 0b00000010; // 2
            byte mask3 = 0b00000100; // 4
            byte mask4 = 0b00100000; // 32
            byte mask5 = 0b01000000; // 64

            if ((resultValue & mask1) != 0)
            {
                conveyorspeed.Value = 50; // mask1일 때 50
                conveyorspeed2.Value = 50;
            }
            else if ((resultValue & mask2) != 0)
            {
                conveyorspeed.Value = 70; // mask2일 때 70
                conveyorspeed2.Value = 70;
            }
            else if ((resultValue & mask3) != 0)
            {
                conveyorspeed.Value = 100; // mask3일 때 100
                conveyorspeed2.Value = 100;
            }
            else if ((resultValue & mask4) != 0)
            {
                conveyorspeed.Value = 70; // mask4일 때 70
                conveyorspeed2.Value = 70;
            }
            else if ((resultValue & mask5) != 0)
            {
                conveyorspeed.Value = 50; // mask5일 때 50
                conveyorspeed2.Value = 50;
            }
            else
            {
                // 기본값 (어떤 마스크 값도 해당되지 않을 때)
                conveyorspeed.Value = 0;
                conveyorspeed2.Value = 0;
            }
        }

    }
}