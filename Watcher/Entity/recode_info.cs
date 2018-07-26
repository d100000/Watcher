using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Entity
{
    public class recode_info
    {
        private long _spend_time;

        public Int64 record_id { get; set; }
        public Int64 uid { get; set; }
        public Int64 date { get; set; }
        public Int64 begin_time { get; set; }
        public Int64 end_time { get; set; }

        public Int64 spend_time
        {
            get => end_time-begin_time;
            set => _spend_time = value;
        }

        public string last_prosses_id { get; set; }
        public string last_prosses_title { get; set; }
        public string this_prosses_id { get; set; }
        public string this_prosses_title { get; set; }
        public string this_prosses_name { get; set; }
        public Int64 create_time { get; set; }


    }
}
