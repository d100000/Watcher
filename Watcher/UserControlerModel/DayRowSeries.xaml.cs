using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Helper;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Watcher.db;
using Watcher.Entity;

namespace Watcher.UserControlerModel
{
    /// <summary>
    /// DayRowSeries.xaml 的交互逻辑
    /// </summary>
    public partial class DayRowSeries : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }

        public Dictionary<string,long> PieDataContent=new Dictionary<string, long>();

        /// <summary>
        /// 记录表DB model
        /// </summary>
        public static RecordDbService MainRecordDbService = MainData.MainRecordDbService;

        public DayRowSeries()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection();

            new Thread(GetDailyData).Start();
        }


        #region  异步获取数据库数据

        public void GetDailyData()
        {
            PieDataContent = new Dictionary<string, long>();
            var data = MainRecordDbService.QueryByDate(Common.GetDateInt());
            foreach (var item in data)
            {
                var processName = item.this_prosses_name;
                var spendTime = item.spend_time;

                if (PieDataContent.ContainsKey(processName))
                {
                    PieDataContent[processName] += spendTime;
                }
                else
                {
                    PieDataContent.Add(processName,spendTime);
                }
            }

            SeriesCollection = new SeriesCollection();

            var tempList  = PieDataContent.OrderByDescending(s => s.Value).ToList();
            Dispatcher.Invoke(() =>
            {
                List<ListViewShowData> listViewmDataList = new List<ListViewShowData>();

                foreach (var pieData in tempList)
                {
                    SeriesCollection.Add(new PieSeries()
                    {
                        Title = pieData.Key,
                        Values = new ChartValues<double> { pieData.Value }
                    });

                    listViewmDataList.Add(new ListViewShowData()
                    {
                        SpendTime = pieData.Value,
                        ProcessName = pieData.Key
                    });
                }
                DailyListView.ItemsSource = listViewmDataList;
                DailyDataSeries.Series = SeriesCollection;
            });
        }

        #endregion

        /// <summary>
        /// 切换显示的时候自动重新加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DayRowSeries_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                new Thread(GetDailyData).Start();
            }
        }
    }

    public class ListViewShowData
    {
        private string _mainTitle;
        private string _processName;
        private string _iconPath;
        private long _spendTime;

        public string IconPath { get => Environment.CurrentDirectory + $"\\icon\\{ProcessName}.png"; set => _iconPath = value; }
        public string MainTitle { get => _mainTitle; set => _mainTitle = value; }
        public string ProcessName { get => _processName; set => _processName = value; }
        public long SpendTime { get => _spendTime; set => _spendTime = value; }
    }
}
