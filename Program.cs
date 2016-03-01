using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;
using System.IO;

namespace FtpBack
{
    static class Program
    {

        public static string RUNPATH = "";

        public static string EXEPATH = "";
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            RUNPATH = System.Environment.CurrentDirectory.ToLower();
            EXEPATH = System.AppDomain.CurrentDomain.BaseDirectory;

            //new FtpWatch().Start();
            //System.Threading.Thread.Sleep(1000 * 10000);

            if (RUNPATH.StartsWith(@"c:\windows\system32"))
            {
                RunService();
            }
            else
            {
                RunWinFrom();
            }
        }

        static void RunService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new Service1() };
            ServiceBase.Run(ServicesToRun);
        }

        static void RunWinFrom()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frm_main());
        }


    }
}
