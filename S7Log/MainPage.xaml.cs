using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Sharp7;
using System.Globalization;

namespace S7Log
{
    // ATO Engineering d.o.o. S7 data logger
    // Thank you Davide Nardella for Snap7 (Sharp7)

    public sealed partial class MainPage : Page
    {
        S7Client Client;
        byte[] Buffer = new byte[65536];
        int SingleRecordSize;
        int DataBlockSize;
        bool MinuteTick;
        bool MinuteTickDone;
        bool HourTick;
        bool HourTickDone;
        bool DayTick;
        bool DayTickDone;
        bool MonthTick;
        bool MonthTickDone;
        bool YearTick;
        bool YearTickDone;

        //CultureInfo.CurrentCulture = new CultureInfo();
        DateTimeFormatInfo DTFormat = CultureInfo.GetCultureInfo("hr-HR").DateTimeFormat;

        public MainPage()
        {
            // Initialize component
            this.InitializeComponent();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
            dispatcherTimer.Start();

            // Get the application view title bar
            ApplicationViewTitleBar appTitleBar = ApplicationView.GetForCurrentView().TitleBar;

            // Set the title bar background and forground color
            appTitleBar.BackgroundColor = Windows.UI.Colors.SteelBlue;
            appTitleBar.ForegroundColor = Windows.UI.Colors.Snow;

            // Set the title bar inactive colors
            appTitleBar.InactiveBackgroundColor = Windows.UI.Colors.LightSteelBlue;
            appTitleBar.InactiveForegroundColor = Windows.UI.Colors.Snow;

            // Set the title bar button colors
            appTitleBar.ButtonBackgroundColor = Windows.UI.Colors.SteelBlue;
            appTitleBar.ButtonForegroundColor = Windows.UI.Colors.Snow;

            // Title bar button hover state colors
            appTitleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.Red;
            appTitleBar.ButtonHoverForegroundColor = Windows.UI.Colors.Snow;

            // Title bar button inctive state colors
            appTitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.LightSteelBlue;
            appTitleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Snow;

            // Title bar button pressed state colors
            appTitleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.Red;
            appTitleBar.ButtonPressedForegroundColor = Windows.UI.Colors.Snow;

            Client = new S7Client();
            Initialize();
            BrowseMode();
        }

        private void Initialize()
        {
            MinuteTick = false;
            MinuteTickDone = false;
            HourTick = false;
            HourTickDone = false;
            DayTick = false;
            DayTickDone = false;
            MonthTick = false;
            MonthTickDone = false;
            YearTick = false;
            YearTickDone = false;

            SingleRecordSize = (8 + (ioValues.SelectedIndex + 1) * 4);
            DataBlockSize = SingleRecordSize * 744;
            ioSize.Text = DataBlockSize.ToString();

        }

        private void BrowseMode()
        {
            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
            btnRead.IsEnabled = false;
        }

        private void RunMode()
        {
            btnConnect.IsEnabled = false;
            btnDisconnect.IsEnabled = true;
            btnRead.IsEnabled = true;
        }

        private void Connect()
        {
            // Spoji se na PLC
            try
            {
                int Result = Client.ConnectTo(ioAddress.Text, Convert.ToInt32(ioRack.Text), Convert.ToInt32(ioSlot.Text));
                ShowResult(Result);
                if (Result == 0)
                {
                    RunMode();
                }
            }
            catch (Exception ex)
            {
            }

        }

        private void Disconnect()
        {
            // Odspoji se sa PLC-a
            try
            {
                int Result = Client.Disconnect();
                ShowResult(Result);
                BrowseMode();
            }
            catch (Exception ex)
            {
            }
        }

        private void Read()
        {
            // Procitaj podatke sa PLC-a
            try
            {
                int Size = Convert.ToInt32(ioSize.Text);
                int Result = Client.DBRead(Convert.ToInt32(ioDB.Text), 0, Size, Buffer);
                ShowResult(Result);
                if (Result == 0)
                {
                    Interpret(Buffer, Size);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void Interpret(byte[] bytes, int size)
        {
            int hourIndex, j;
            string[,] dataArray = new string[744, 2 + ioValues.SelectedIndex];

            if (bytes == null)
                return;

            byte[] bTSYear = new byte[2];
            byte[] bTSMonth = new byte[2];
            byte[] bTSDay = new byte[2];
            byte[] bTSHour = new byte[2];
            byte[] bTSMinute = new byte[2];
            byte[] bTSSecond = new byte[2];
            byte[] bValue1 = new byte[4];
            byte[] bValue2 = new byte[4];
            byte[] bValue3 = new byte[4];
            byte[] bValue4 = new byte[4];
            byte[] bValue5 = new byte[4];

            for (hourIndex = 0; hourIndex <= 743; hourIndex++)
            {
                int pointer = hourIndex * SingleRecordSize;

                bTSYear[0] = bytes[pointer + 1];
                bTSYear[1] = bytes[pointer + 0];

                bTSMonth[0] = bytes[pointer + 2];
                bTSMonth[1] = 0;

                bTSDay[0] = bytes[pointer + 3];
                bTSDay[1] = 0;

                bTSHour[0] = bytes[pointer + 4];
                bTSHour[1] = 0;

                bTSMinute[0] = bytes[pointer + 5];
                bTSMinute[1] = 0;

                bTSSecond[0] = bytes[pointer + 6];
                bTSSecond[1] = 0;

                int TSYear = System.BitConverter.ToInt16(bTSYear, 0);
                int TSMonth = System.BitConverter.ToInt16(bTSMonth, 0);
                int TSDay = System.BitConverter.ToInt16(bTSDay, 0);
                int TSHour = System.BitConverter.ToInt16(bTSHour, 0);
                int TSMinute = System.BitConverter.ToInt16(bTSMinute, 0);
                int TSSecond = System.BitConverter.ToInt16(bTSSecond, 0);
                string TS = TSDay.ToString() + '.' + TSMonth.ToString() + '.' + TSYear.ToString() + ' ' + TSHour.ToString() + ":" + TSMinute.ToString() + ":" + TSSecond.ToString();
                dataArray[hourIndex, 0] = TS;

                if (ioValues.SelectedIndex >= 0)
                {
                    bValue1[0] = bytes[pointer + 11];
                    bValue1[1] = bytes[pointer + 10];
                    bValue1[2] = bytes[pointer + 9];
                    bValue1[3] = bytes[pointer + 8];
                    float Value1 = (float)Math.Round(System.BitConverter.ToSingle(bValue1, 0), 2);
                    string VAL1 = Value1.ToString();
                    dataArray[hourIndex, 1] = VAL1;
                }
                if (ioValues.SelectedIndex >= 1)
                {
                    bValue2[0] = bytes[pointer + 15];
                    bValue2[1] = bytes[pointer + 14];
                    bValue2[2] = bytes[pointer + 13];
                    bValue2[3] = bytes[pointer + 12];
                    float Value2 = (float)Math.Round(System.BitConverter.ToSingle(bValue2, 0), 2);
                    string VAL2 = Value2.ToString();
                    dataArray[hourIndex, 2] = VAL2;
                }
                if (ioValues.SelectedIndex >= 2)
                {
                    bValue3[0] = bytes[pointer + 19];
                    bValue3[1] = bytes[pointer + 18];
                    bValue3[2] = bytes[pointer + 17];
                    bValue3[3] = bytes[pointer + 16];
                    float Value3 = (float)Math.Round(System.BitConverter.ToSingle(bValue3, 0), 2);
                    string VAL3 = Value3.ToString();
                    dataArray[hourIndex, 3] = VAL3;
                }
                if (ioValues.SelectedIndex >= 3)
                {
                    bValue4[0] = bytes[pointer + 23];
                    bValue4[1] = bytes[pointer + 22];
                    bValue4[2] = bytes[pointer + 21];
                    bValue4[3] = bytes[pointer + 20];
                    float Value4 = (float)Math.Round(System.BitConverter.ToSingle(bValue4, 0), 2);
                    string VAL4 = Value4.ToString();
                    dataArray[hourIndex, 4] = VAL4;
                }
                if (ioValues.SelectedIndex >= 4)
                {
                    bValue5[0] = bytes[pointer + 27];
                    bValue5[1] = bytes[pointer + 26];
                    bValue5[2] = bytes[pointer + 25];
                    bValue5[3] = bytes[pointer + 24];
                    float Value5 = (float)Math.Round(System.BitConverter.ToSingle(bValue5, 0), 2);
                    string VAL5 = Value5.ToString();
                    dataArray[hourIndex, 5] = VAL5;
                }

                WriteCSV(dataArray);
            }
        }

        private void ShowResult(int Result)
        {
            // This function returns a textual explaination of the error code
            ioResult.Text = Client.ErrorText(Result);
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            Read();
        }

        private void CBValues_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Recalculate DB size
            SingleRecordSize = (8 + (ioValues.SelectedIndex + 1) * 4);
            DataBlockSize = SingleRecordSize * 744;
            ioSize.Text = DataBlockSize.ToString();
        }

        private void DispatcherTimer_Tick(object sender, object e)

        {
            // Zapamti cijeli tick vremenski zig (da se ne cita DateTime.Now u kodu) i osvjezi sat
            DateTime tickTimeStamp = DateTime.Now;          
            ioStatus.Text = DateTime.Now.ToString(DTFormat);

            // Ukoliko je prosao tick nulte sekunde resetiraj flagove periodickih zadataka
            if (tickTimeStamp.Second > 0)
            {
                MinuteTickDone = false;
                HourTickDone = false;
                DayTickDone = false;
                MonthTickDone = false;
                YearTickDone = false;
            }

            // Prozovi periodicke zadatak i postavi flag kada su izvrseni
            if (MinuteTickDone == false && tickTimeStamp.Second == 0)
            {
                MinuteTick = true;
            }
            if (HourTickDone == false && tickTimeStamp.Second == 0 && tickTimeStamp.Minute == 0)
            {
                HourTick = true;
            }
            if (DayTickDone == false && tickTimeStamp.Second == 0 && tickTimeStamp.Minute == 0 && tickTimeStamp.Hour == 0)
            {
                DayTick = true;
            }
            if (MonthTickDone == false && tickTimeStamp.Second == 0 && tickTimeStamp.Minute == 0 && tickTimeStamp.Hour == 0 && tickTimeStamp.Day == 1)
            {
                MonthTick = true;
            }
            if (YearTickDone == false && tickTimeStamp.Second == 0 && tickTimeStamp.Minute == 0 && tickTimeStamp.Hour == 0 && tickTimeStamp.Day == 1 && tickTimeStamp.Month == 1)
            {
                YearTick = true;
            }

            // Ukoliko je bilo koji periodicki task okinut procitaj podatke iz PLC-a
            if (MinuteTick || HourTick || DayTick || MonthTick || YearTick)
            {
                Connect();
                Read();
                Disconnect();

                if (MinuteTick) MinuteTask();
                if (HourTick) HourTask();
                if (DayTick) DayTask();
                if (MonthTick) MonthTask();
                if (YearTick) YearTask();
            }

            // Pocisti buffer iza sebe
            Array.Clear(Buffer, 0, Buffer.Length);
        }

        private void MinuteTask()
        {
            // Postavi kontrolni bit da je task izvrsen
            MinuteTick = false;
            MinuteTickDone = true;

            // Kasnije upisi u log negdje
            // ioDump.Text += "Minutni zadatak se izvršio u " + DateTime.Now.ToString(DTFormat) + "\r\n";
        }

        private void HourTask()
        {
            // Postavi kontrolni bit da je task izvrsen
            HourTick = false;
            HourTickDone = true;

            // Kasnije upisi u log negdje
            // ioDump.Text += "Satni zadatak se izvršio u " + DateTime.Now.ToString(DTFormat) + "\r\n";
        }

        private void DayTask()
        {
            // Postavi kontrolni bit da je task izvrsen
            DayTick = false;
            DayTickDone = true;

            // Kasnije upisi u log negdje
            // ioDump.Text += "Dnevni zadatak se izvršio u " + DateTime.Now.ToString(DTFormat) + "\r\n";
        }

        private void MonthTask()
        {
            // Postavi kontrolni bit da je task izvrsen
            MonthTick = false;
            MonthTickDone = true;

            // Kasnije upisi u log negdje
            // ioDump.Text += "Mjesečni zadatak se izvršio u " + DateTime.Now.ToString(DTFormat) + "\r\n";
        }

        private void YearTask()
        {
            // Postavi kontrolni bit da je task izvrsen
            YearTick = false;
            YearTickDone = true;

            // Kasnije upisi u log negdje
            // ioDump.Text += "Godišnji zadatak se izvršio u " + DateTime.Now.ToString(DTFormat) + "\r\n";
        }

        private void WriteCSV(string[,] dataArray)
        {
                        
        }

    }
}