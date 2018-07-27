
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
        private Icon _processIcon;


        public IntPtr Id { get => _id; set => _id = value; }
        public Process ProcessInfoData { get => _processInfoData; set => _processInfoData = value; }
        public Icon ProcessIcon { get => _processIcon; set => _processIcon = value; }




    }
}