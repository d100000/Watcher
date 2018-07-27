using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

// system prosses

using System.Diagnostics;
using System.Drawing;
using System.Threading;

using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Helper;
using Watcher.db;
using Watcher.Entity;
using Application = System.Windows.Application;

namespace Watcher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {

        public List<string> ProcessList = new List<string>();
        public List<string> ProcessListAll = new List<string>();
        public Dictionary<IntPtr, ProcessInfo> RunningDictionary = new Dictionary<IntPtr, ProcessInfo>();

        public RecordDbService MainRecordDbService;

        public bool MonitorSwitch = true;

        public IntPtr NowProssesId;

        public Process NowProcess;

        private bool _timmerLock;

        private recode_info _currentRecordInfo ;

        private readonly System.Timers.Timer _t = new System.Timers.Timer(1000);

        #region  Windows API

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private const int MaxPath = 260;
        public const int ProcessAllAccess = 0x000F0000 | 0x00100000 | 0xFFF;
        [DllImport("coredll.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);
        [DllImport("coredll.dll")]
        public static extern IntPtr OpenProcess(int fdwAccess, int fInherit, int IDProcess);
        [DllImport("coredll.dll")]
        public static extern bool TerminateProcess(IntPtr hProcess, int uExitCode);
        [DllImport("coredll.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Coredll.dll", EntryPoint = "GetModuleFileName")]
        private static extern uint GetModuleFileName(IntPtr hModule, [Out] StringBuilder lpszFileName, int nSize);
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern IntPtr ExtractIconEx(string fileName, int index, ref IntPtr hIconLarge, ref IntPtr hIconSmall, uint nIcons);

        #endregion


        public MainWindow()
        {
            InitializeComponent();
            MainRecordDbService = new RecordDbService();// 初始化

            SetIcon();// 最小化

            _t.Elapsed += ProcessTimer;

            //到达时间的时候执行事件；   

            _t.AutoReset = true;

            _t.Enabled = true;
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
                try
                {
                    var info = p.Id + "  " + p.ProcessName + "  " + p.MainWindowTitle + "  ";
                    ProcessListAll.Add(info);
                    if (p.MainWindowHandle != IntPtr.Zero && p.MainWindowTitle != "")
                    {
                        Icon icon = GetSmallIconFromHandle(p.MainWindowHandle);

                        RunningDictionary.Add(p.MainWindowHandle, new ProcessInfo()
                        {
                            ProcessInfoData = p,
                            ProcessIcon = icon

                        });
                        ProcessList.Add(p.MainWindowTitle);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

           string activiteProssesName = RunningDictionary.ContainsKey(activiteNo)
               ? RunningDictionary[activiteNo].ProcessInfoData.MainWindowTitle
               : "未选中应用";

            // 切换应用时触发
            if (RunningDictionary.ContainsKey(activiteNo) && activiteNo != NowProssesId)
            {
                if (_currentRecordInfo != null)
                {
                    // 更新旧进程的结束时间
                    var currentTime = Common.GetTimeStamp(DateTime.Now);
                    MainRecordDbService.Update(new { end_time = currentTime, spend_time = (currentTime - _currentRecordInfo.begin_time) }, new { _currentRecordInfo.record_id });
                }

                // 插入新进程的数据
                var runningProcess = RunningDictionary[activiteNo].ProcessInfoData;
                _currentRecordInfo=MainRecordDbService.AddNewRecord(runningProcess, NowProcess);
                NowProssesId = activiteNo;
                NowProcess = runningProcess;

                try
                {
                    Dispatcher.Invoke(() => {
                        ForegroundTextBox.Text = activiteProssesName;
                        MainListBox.ItemsSource = RunningDictionary.Values.Select(c => c.ProcessInfoData.MainWindowTitle);
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                if (_countUpdate == 5)// 五秒主动更新一次数据库
                {
                    var currentTime = Common.GetTimeStamp(DateTime.Now);
                    MainRecordDbService.Update(new { end_time = currentTime, spend_time = (currentTime - _currentRecordInfo.begin_time) }, new {_currentRecordInfo.record_id });
                    _countUpdate = 0;
                }
                _countUpdate++;
            }

            SetMouseInfo();

            _timmerLock = false;

        }

        #region  系统方法

        /// <summary>
        /// 通过句柄获取运行程序路径
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static String GetAppRunPathFromHandle(IntPtr hwnd)
        {
            int pId = 0;
            GetWindowThreadProcessId(hwnd, ref pId);
            var pHandle = OpenProcess(ProcessAllAccess, 0, pId);
            StringBuilder sb = new StringBuilder(MaxPath);
            GetModuleFileName(pHandle, sb, sb.Capacity);
            CloseHandle(pHandle);
            return sb.ToString();
        }

        /// <summary>
        /// 根据句柄获取运行程序小图标 //当然也可以获取大图标
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        private Icon GetSmallIconFromHandle(IntPtr hwnd)
        {
            IntPtr hLargeIcon = IntPtr.Zero;
            IntPtr hSmallIcon = IntPtr.Zero;
            String filePath = GetAppRunPathFromHandle(hwnd);

            ExtractIconEx(filePath, 0, ref hLargeIcon, ref hSmallIcon, 1);
            Icon icon = null;
            try
            {
                icon = System.Drawing.Icon.FromHandle(hSmallIcon);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
            return icon;
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
                        create_time = Common.GetTimeStamp(),
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
            MainRecordDbService.Update(new { end_time = currentTime, spend_time = (currentTime - _currentRecordInfo.begin_time) }, new {_currentRecordInfo.record_id });
            _notifyIcon.Visible = false;
            Hide();
            GetProssesList();
            Environment.Exit(0);
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
    }
}
