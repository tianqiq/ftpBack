using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace FtpBack
{
    partial class Service1 : ServiceBase
    {

        public Service1()
        {
            InitializeComponent();
        }

        private FtpWatch watch = new FtpWatch();

        protected override void OnStart(string[] args)
        {
           watch.Start();
        }

        protected override void OnStop()
        {
            watch.Stop();
        }
    }
}
