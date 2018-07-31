
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Watcher.Entity
{
    public class ProcessInfo
    {
        private IntPtr _id;
        private Process _processInfoData;
        private Image _processIcon;
        private string _mainTitle;
        private string _iconPath;

        private long _spendTime;

        public IntPtr Id { get => _id; set => _id = value; }
        public Process ProcessInfoData { get => _processInfoData; set => _processInfoData = value; }
        public Image ProcessIcon { get => _processIcon; set => _processIcon = value; }
        public string MainTitle { get => _processInfoData.MainWindowTitle;  }
        public string IconPath { get => Environment.CurrentDirectory + $"\\icon\\{_processInfoData.ProcessName}.png"; set => _iconPath = value; }
        public long SpendTime { get => _spendTime; set => _spendTime = value; }
    }
}