using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace FtpBack
{
    class BackWork
    {
        public BackWorkConfig config;

        public Dictionary<String, UpLoad> Ups = new Dictionary<string, UpLoad>();

        public BackWork(BackWorkConfig config)
        {
            Name = config.name;
            this.config = config;

        }

        private FileSystemWatcher watch;

        public String Name;

        private string watchPath;

        private System.Threading.Timer timer;
        private void Ini()
        {
            UpLoad up;
            if (config.childs != null)
            {
                foreach (var item in config.childs)
                {
                    if (item.Value.run && item.Value.ftp != null && !string.IsNullOrEmpty(item.Key))
                    {
                        var workName = config.name + "(" + item.Key + ")";
                        up = new UpLoad(workName, item.Value.ftp);
                        up.Dir = item.Key;
                        up.FullDir = config.dir + "/" + item.Key;
                        up.IsChild = true;
                        Ups.Add(workName, up);
                        up.Start();
                    }
                }
            }
            else
            {
                up = new UpLoad(config.name, config.ftp);
                up.FullDir = up.Dir = config.dir;
                up.Dir = config.ftp.dir;
                up.IsChild = false;
                Ups.Add(config.name, up);
                up.Start();
            }


            watch = new FileSystemWatcher();
            watch.Filter = config.fiter;
            watch.IncludeSubdirectories = true;
            watch.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
            watch.InternalBufferSize = 16 * 1024 * 2;
            watch.Created += new FileSystemEventHandler(watch_Changed);
            watch.Changed += new FileSystemEventHandler(watch_Changed);
            watchPath = watch.Path = config.dir;
            timer = new System.Threading.Timer(FtpBack, null, Timeout.Infinite, Timeout.Infinite);
        }


        private void FtpBack(object o)
        {
            foreach (var item in eventCache)
            {
                addFile(item.Value.Name.Replace('\\', '/'), item.Value.FullPath.Replace('\\', '/'));
            }
            Thread.Sleep(3 * 1000);
            WriteRunLastTime();
            lock (eventCache)
            {
                eventCache.Clear();
            }
        }

        private Dictionary<string, FileSystemEventArgs> eventCache = new Dictionary<string, FileSystemEventArgs>();

        int i = 0;
        private void watch_Changed(object sender, FileSystemEventArgs e)
        {
            var workNmae = pathToWorkName(e.Name);
            if (!Directory.Exists(e.FullPath) && Ups.ContainsKey(workNmae))
            {
                lock (eventCache)
                {
                    if (!eventCache.ContainsKey(e.FullPath))
                    {
                        eventCache.Add(e.FullPath, e);
                        timer.Change(3000, Timeout.Infinite);
                    }
                }
            }
        }


        private string pathToWorkName(string path)
        {
            var firstPath = path.Replace("\\", "/").Split('/');
            var key = config.name + "(" + firstPath[0] + ")";
            if (Ups.ContainsKey(key))
            {
                return key;
            }
            else
            {
                return config.name;
            }
        }

        private string pathToFtpPath(string path, UpLoad up)
        {
            if (up.IsChild)
            {
                return path.Substring(up.Dir.Length + 1);
            }
            else
            {
                return path;
            }
        }

        private void addFile(string ftpPath, string localPath)
        {
            var workName = pathToWorkName(ftpPath);
            UpLoad up;
            if (Ups.TryGetValue(workName, out up))
            {
                var info = new System.IO.FileInfo(localPath);
                var file = new FileInfo() { WorkName = workName, FtpPath = pathToFtpPath(ftpPath, up), LocalPath = localPath, Size = Math.Round((double)info.Length / 1024 * 1.0, 3) };
                file.AddTime = DateTime.Now;
                up.AddFile(file);
            }
        }


        public void ScaPath(string path, DateTime runLastTime)
        {
            var dirInfo = new DirectoryInfo(path);

            if (!dirInfo.Exists)
            {
                return;
            }

            foreach (var item in dirInfo.GetFiles())
            {
                if (item.LastWriteTime > runLastTime || item.CreationTime > runLastTime)
                {
                    addFile(item.FullName.Substring(watchPath.Length + 1).Replace('\\', '/'), item.FullName.Replace('\\', '/'));
                }
            }

            foreach (var item in dirInfo.GetDirectories())
            {
                ScaPath(item.FullName, runLastTime);
            }
        }

        public void ScanCurrentFile()
        {
            foreach (var item in Ups)
            {
                ScaPath(item.Value.FullDir, item.Value.GetLastTime());
            }
        }



        public void Start()
        {
            Ini();
            ScanCurrentFile();
            watch.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            WriteRunLastTime();
        }

        public void WriteRunLastTime()
        {
            foreach (var item in Ups)
            {
                item.Value.WriteRunLastTime();
            }
        }


    }
}
