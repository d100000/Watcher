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
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace Watcher.UserControlerModel
{
    /// <summary>
    /// DayRowSeries.xaml 的交互逻辑
    /// </summary>
    public partial class DayRowSeries : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public Dictionary<string, int> RowKeyDictionary=new Dictionary<string, int>();

        public DayRowSeries()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50 }
                }
            };

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };

            Task.Run(() =>
            {
                var r = new Random();
                while (true)
                {
                    Thread.Sleep(3000);
                    double _trend =r.Next(5, 100);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var tempList= Labels.ToList();
                        tempList.Add(_trend.ToString());
                        Labels = tempList.ToArray();
                        SeriesCollection[0].Values.Add(_trend);

                        DataContext = this;
                    });
                }
            });

        }
    }
}
