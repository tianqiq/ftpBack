using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FtpBack
{
    class FtpWatch
    {

        private List<BackWork> works = new List<BackWork>();

        public void Start()
        {
            System.Net.WebRequest.DefaultWebProxy = null;
            var config = BackWorkConfig.Create(Program.EXEPATH + "/work.json");
            foreach (var item in config)
            {
                var work = new BackWork(item);
                works.Add(work);
                work.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in works)
            {
                item.Stop();
            }
        }
    }
}
