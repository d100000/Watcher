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

            SeriesCollection = new SeriesCollection
            {
                new PieSeries()
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 50 }
                },
                new PieSeries()
                {
                    Title = "2017",
                    Values = new ChartValues<double> { 50 }
                }
            };

            new Thread(GetDailyData).Start();

            DataContext = this;
        }


        #region  异步获取数据库数据

        public void GetDailyData()
        {
            var data = MainRecordDbService.QueryByDate(Common.GetDateInt());
            foreach (var item in data)
            {
                var ProcessName = item.this_prosses_name;
                var SpendTime = item.spend_time;

                if (PieDataContent.ContainsKey(ProcessName))
                {
                    PieDataContent[ProcessName] += SpendTime;
                }
                else
                {
                    PieDataContent.Add(ProcessName,SpendTime);
                }
            }

            SeriesCollection = new SeriesCollection();


            Dispatcher.Invoke(() =>
            {
                foreach (var pieData in PieDataContent)
                {
                    SeriesCollection.Add(new PieSeries()
                    {
                        Title = pieData.Key,
                        Values = new ChartValues<double> { pieData.Value }
                    });
                }

                DataContext = this;
            });
        }



        #endregion 

    }
}
