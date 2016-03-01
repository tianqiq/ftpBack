using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FtpBack
{
    class FileInfo
    {

        public string WorkName { get; set; }

        public int ID { get; set; }

        public string FtpPath { get; set; }

        public string LocalPath { get; set; }

        public double Size { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime UpTime { get; set; }

    }
}
