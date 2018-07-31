using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

// system prosses

using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using Helper;
using LiveCharts;
using LiveCharts.Wpf;
using Watcher.db;
using Watcher.Entity;
using Application = System.Windows.Application;
using Control = System.Windows.Forms.Control;
using MessageBox = System.Windows.MessageBox;

namespace Watcher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {

        public List<string> ProcessList = new List<string>();

        public Dictionary<IntPtr, ProcessInfo> RunningDictionary = new Dictionary<IntPtr, ProcessInfo>();
        /// <summary>
        /// 记录表DB model
        /// </summary>
        public static RecordDbService MainRecordDbService=MainData.MainRecordDbService;

        public bool MonitorSwitch = true;

        public IntPtr NowProssesId;

        public Process NowProcess;

        private bool _timmerLock;

        private recode_info _currentRecordInfo;

        private readonly System.Timers.Timer _t = new System.Timers.Timer(1000);

        #region Rowseries

        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }

        #endregion 

        #region  Windows API

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            SetIcon();// 最小化

            InitComboBox();// 初始化下拉框

            _t.Elapsed += ProcessTimer;

            //到达时间的时候执行事件；   

            _t.AutoReset = true;

            _t.Enabled = true;

            // 主页面条形图数据
            SeriesCollection = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "当前运行情况",
                    Values = new ChartValues<long> {  }
                }
            };
            Labels = new List<string>();
            try
            {
                GetProssesList();
            }
            catch (Exception e)
            {
                MessageBox.Show("内部启动错误GetProssesList: " + e.Message + e.StackTrace, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        
        }

        public void ProcessTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!_timmerLock)
            {
                new Thread(GetProssesList).Start();
            }

        }

        private int _countUpdate;

        public void GetProssesList()
        {
            _timmerLock = true;
            var ps = Process.GetProcesses(Environment.MachineName);
            ProcessList = new List<string>();
            RunningDictionary = new Dictionary<IntPtr, ProcessInfo>();
            var activiteNo = GetForegroundWindow();
            foreach (var p in ps)
            {
                if (p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle != "" && !RunningDictionary.ContainsKey(p.MainWindowHandle))
                {
                    try
                    {
                        GetIcon(p.MainModule.FileName, p.ProcessName);
                    }
                    catch (Exception ex)
                    {
                        
                    }

                    RunningDictionary.Add(p.MainWindowHandle, new ProcessInfo()
                    {
                        ProcessInfoData = p,
                    });
                    ProcessList.Add(p.MainWindowTitle);
                }

            }

            string activiteProssesName = RunningDictionary.ContainsKey(activiteNo)
                ? RunningDictionary[activiteNo].ProcessInfoData.MainWindowTitle
                : "未选中应用";
            var currentTime = Common.GetTimeStamp(DateTime.Now);
            // 切换应用时触发
            if (RunningDictionary.ContainsKey(activiteNo) && activiteNo != NowProssesId)
            {
                if (_currentRecordInfo != null)
                {
                    // 更新旧进程的结束时间
                    MainRecordDbService.Update(new { end_time = currentTime, spend_time = (currentTime - _currentRecordInfo.begin_time) }, new { _currentRecordInfo.record_id });
                }

                // 插入新进程的数据
                var runningProcess = RunningDictionary[activiteNo].ProcessInfoData;
                _currentRecordInfo = MainRecordDbService.AddNewRecord(runningProcess, NowProcess);
                NowProssesId = activiteNo;
                NowProcess = runningProcess;

                // 添加前端变化
                SeriesCollection[0].Values.Add(currentTime - _currentRecordInfo.begin_time+1);
                Labels.Add(NowProcess.ProcessName);

                if (SeriesCollection[0].Values.Count == 9)
                {
                    SeriesCollection[0].Values.RemoveAt(0);
                    Labels.RemoveAt(0);
                }

                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        ForegroundTextBox.Text = activiteProssesName;
                        MainListBox.ItemsSource = RunningDictionary.Values;
                        MainRowSeries.Series = SeriesCollection;

                        DataContext = this;// 触发UI 进程进行数据刷新
                    });
                }
                catch (Exception e)
                {
                    MessageBox.Show("内部启动错误: " + e.Message + e.Data, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    if (SeriesCollection[0].Values.Count > 0)
                    {
                        SeriesCollection[0].Values[SeriesCollection[0].Values.Count - 1] =
                            Common.GetTimeStamp(DateTime.Now) - _currentRecordInfo.begin_time;
                    }

                });

                if (_countUpdate == 5&& _currentRecordInfo!=null)// 五秒主动更新一次数据库
                {
                    currentTime = Common.GetTimeStamp(DateTime.Now);
                    MainRecordDbService.Update(new { end_time = currentTime, spend_time = (currentTime - _currentRecordInfo.begin_time) }, new { _currentRecordInfo.record_id });
                    _countUpdate = 0;
                }
                // 更新图表信息
                Dispatcher.Invoke(() =>
                {

                    MainRowSeries.Series = SeriesCollection;
                    DataContext = this;
                });
                _countUpdate++;
            }

            SetMouseInfo();

            _timmerLock = false;

        }

        #region  系统方法


        /// <summary>
        /// 通过程序路径获取图标信息
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public Icon GetIcon(string FilePath, string appName)
        {

            var returnData = new List<Icon>();
            var imageList = new ImageList();

            var icon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath); //图标添加进imageList中
            imageList.Images.Add(icon);
            returnData.Add(icon);

            if (!Directory.Exists(Environment.CurrentDirectory + $"\\icon"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + $"\\icon");
            }

            if (!File.Exists(Environment.CurrentDirectory + $"\\icon\\{appName}.png"))
            {
                System.IO.FileStream fs = new System.IO.FileStream(Environment.CurrentDirectory + $"\\icon\\{appName}.png", System.IO.FileMode.Create);
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                imageList.Images[0].Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                fs.Close();
            }
            return returnData[0];
        }

        #endregion

        #region 最小化隐藏

        NotifyIcon _notifyIcon;


        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WatcherMainWindows.WindowState != WindowState.Minimized) return;
            Hide();
            NotifyTips("主人，我隐藏在了右下角哦！");
        }

        /// <summary>
        /// 设置最小化 icon
        /// </summary>
        public void SetIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                BalloonTipText = @"软件启动啦，主人！",
                Text = @"守望者程序 Watcher",
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                Visible = true
            };

            _notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
            _notifyIcon.ShowBalloonTip(1000);

            var cms = new ContextMenuStrip();
            //关联 NotifyIcon 和 ContextMenuStrip
            _notifyIcon.ContextMenuStrip = cms;

            var exitMenuItem = new ToolStripMenuItem { Text = @"退出程序", ToolTipText = @"关闭程序" };
            exitMenuItem.Click += exitMenuItem_Click;

            cms.Items.Add(exitMenuItem);

        }
        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;// 用于触发窗口最前端
            Activate();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            NotifyTips("主人我没有消失，我在右下角哦！");
        }


        #endregion

        #region 判断鼠标激活事件

        public string MouseLocation;

        public long MouseStopTime;

        public int MouseStopCount;

        public void SetMouseInfo()
        {
            int x = Control.MousePosition.X;
            int y = Control.MousePosition.Y;

            var currentLocation = x + "_" + y;
            if (currentLocation == MouseLocation)
            {
                if (MouseStopCount == 0)
                {
                    MouseStopTime = Common.GetTimeStamp(DateTime.Now);
                }
                MouseStopCount++;
            }
            else
            {
                if (MouseStopCount > 10)// 鼠标停止湿巾大于10秒
                {
                    MainRecordDbService.Insert(new recode_info()
                    {
                        date = Common.GetDateInt(),
                        begin_time = MouseStopTime,
                        end_time = Common.GetTimeStamp(),
                        this_prosses_id = NowProcess.Id.ToString(),
                        this_prosses_name = @"鼠标静止",
                        this_prosses_title = @"UnActivate",
                        create_time = Common.GetTimeStamp().ToString(),
                    });
                }
                MouseLocation = currentLocation;
                MouseStopCount = 0;
            }


        }


        #endregion 

        /// <summary>
        /// 退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            // 记录退出时间
            var currentTime = Common.GetTimeStamp(DateTime.Now);
            MainRecordDbService.Update(new { end_time = currentTime, spend_time = (currentTime - _currentRecordInfo.begin_time) }, new { _currentRecordInfo.record_id });
            _notifyIcon.Visible = false;
            Hide();
            GetProssesList();
            Environment.Exit(0);
        }

        /// <summary>
        /// 初始化下拉框
        /// </summary>
        public void InitComboBox()
        {
            SelectDateTime.ItemsSource=new List<string>()
            {
                "当天",
                "昨天"
            };
            SelectDateTime.SelectedItem = "当天";
        }

        /// <summary>
        /// 下拉框选项变更触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectDateTime_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (SelectDateTime.SelectedItem)
            {
                case "当天":
                    MainData.SelectDayTime = Common.GetDateInt();
                    break;
                case "昨天":
                    MainData.SelectDayTime = Common.GetDateInt()-1;
                    break;
            }
            DayRowSeriesControler.GetDailyData();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tips"></param>
        public void NotifyTips(string tips)
        {
            _notifyIcon.BalloonTipText = tips;
            _notifyIcon.ShowBalloonTip(5000);
        }

        /// <summary>
        /// 控制全天分析展现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MainPanel.Visibility == Visibility.Hidden)
            {
                MainPanel.Visibility = Visibility.Visible;
                DayRowSeriesControler.Visibility= Visibility.Collapsed;
                SelectDailyStackPanel.Visibility= Visibility.Collapsed;
                ChangeButton.Content = "每日分析";
            }
            else
            {
                MainPanel.Visibility = Visibility.Hidden;
                DayRowSeriesControler.Visibility = Visibility.Visible;
                SelectDailyStackPanel.Visibility = Visibility.Visible;
                ChangeButton.Content = "实时数据";
            }

        }


    }

    public static class MainData{
        public static RecordDbService MainRecordDbService=new RecordDbService();

        public static long SelectDayTime = Common.GetDateInt();
    }
}
