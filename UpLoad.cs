using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FtpBack
{
    class UpLoad
    {
        public string WorkName { get; set; }

        public string Dir { get; set; }

        public string FullDir { get; set; }

        public Boolean IsChild { get; set; }

        public UpLoad(string workName, FtpConfig ftpConfig)
        {
            FileDB = new SqlLiteDB(workName);
            ftp = new SimpleFtpClient(ftpConfig);
            this.WorkName = workName;
        }



        private bool IsRun = false;
        public void Start()
        {
            TriggerUpload();
        }

        private IDB FileDB { get; set; }
        private SimpleFtpClient ftp { get; set; }
        private ManualResetEvent manual = new ManualResetEvent(true);

        public void TriggerUpload()
        {
            lock (this)
            {
                if (!IsRun)
                {
                    ThreadPool.QueueUserWorkItem(innerStart);
                    IsRun = true;
                }
            }
        }


        public void WriteRunLastTime()
        {
            this.FileDB.Set("runLastTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }


        public DateTime GetLastTime()
        {
            var time = this.FileDB.Get("runLastTime");
            return time != null ? DateTime.Parse(time) : DateTime.Now;
        }


        //event Action<int, string, string> OnUploadSuccess;

        //event Action<int, string, string> OnUploadError;

        //event Action<int, string, string> OnUploading;

        public void innerStart(object o)
        {
            FileInfo file = null;
            string error_message;
            while ((file = FileDB.GetOne()) != null)
            {
                if (ftp.UpFile(file.FtpPath, file.LocalPath, out error_message))
                {
                    FileDB.Remove(file.ID);
                    file.UpTime = DateTime.Now;
                    FileDB.ReportUpSuccess(file);
                }
                else
                {
                    FileDB.ReportError(file.ID, error_message);
                }
            }
            lock (this)
            {
                IsRun = false;
            }
        }

        public void AddFile(FileInfo file)
        {
            if (!FileDB.IsExist(file))
            {
                FileDB.Add(file);
                TriggerUpload();
            }
        }


    }
}
