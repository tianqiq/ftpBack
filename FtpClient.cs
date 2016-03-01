using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace FtpBack
{
    class SimpleFtpClient
    {

        public string Host { get; set; }


        public FtpWebRequest createUpFileRequest(string path)
        {

            var req = FtpWebRequest.Create(new Uri("ftp://" + Ftp.host + "/" + Ftp.dir + "/" + path.Replace("\\", "/"))) as FtpWebRequest;
            SetFtpConfig(req);
            return req;
        }

        private FtpConfig Ftp;

        public SimpleFtpClient(FtpConfig ftp)
        {
            this.Ftp = ftp;
        }


        private string Combine(int type = 0, params string[] paths)
        {
            var buff = new StringBuilder();
            foreach (var item in paths)
            {
                buff.AppendFormat("/{0}", item);
            }
            var path = buff.Replace("//", "/").Replace("\\", "/").ToString().Trim('/');
            return type == 0 ? path : path.Replace("/", "\\");
        }


        private HashSet<string> FtpDirCache = new HashSet<string>();

        public bool UpFile(string ftpPath, string localFilePath, out string error_message)
        {

            var ftpDir = Combine(0, Ftp.dir, Path.GetDirectoryName(ftpPath));

            if (!FtpDirCache.Contains(ftpDir))
            {
                CreateFtpDir(ftpDir, true);
            }

            var req = createUpFileRequest(ftpPath);
            req.Method = WebRequestMethods.Ftp.UploadFile;
            try
            {
                using (var ftpStream = req.GetRequestStream())
                {
                    //System.Threading.Thread.Sleep(50);
                    using (var fileStream = File.Open(localFilePath, FileMode.Open, FileAccess.Read))
                    {
                        Copy(fileStream, ftpStream);
                        ftpStream.Flush();
                    }
                }
                req.GetResponse().Close();

                if (!FtpDirCache.Contains(ftpDir))
                {
                    FtpDirCache.Add(ftpDir);
                }

                LogHelp.Info("upfile___success___" + localFilePath + "___" + localFilePath);
                error_message = null;
                return true;
            }
            catch (Exception e)
            {
                LogHelp.Error("upfile___error___" + req.RequestUri.LocalPath + "___" + req.RequestUri.LocalPath + "___" + e.Message);
                error_message = e.Message;
            }
            return false;
        }


        private void Copy(Stream _out, Stream _in)
        {
            var buff = new Byte[2048];
            var len = 0;
            while ((len = _out.Read(buff, 0, 2048)) != 0)
            {
                _in.Write(buff, 0, len);
            }
        }



        public FtpWebRequest createCreateDirRequest(string path)
        {
            var url = "ftp://" + Combine(0, Ftp.host, path) + "/";
            var req = FtpWebRequest.Create(new Uri(url)) as FtpWebRequest;
            SetFtpConfig(req);
            return req;
        }

        private void SetFtpConfig(FtpWebRequest req)
        {
            req.Proxy = null;
            req.Credentials = new NetworkCredential(Ftp.name, Ftp.pwd);
            req.ReadWriteTimeout = 10 * 1000;
            req.UseBinary = true;
            req.UsePassive = Ftp.pasv;
            req.Timeout = 30 * 1000;
        }

        public bool CreateFtpDir(string path, bool check)
        {
            if (check && ExistFtpPath(path))
            {
                return true;
            }
            var req = createCreateDirRequest(path);
            req.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                var rep = req.GetResponse();
                req.GetResponse().Close();
                LogHelp.Info("createDir___success___" + path);
                return true;
            }
            catch (Exception e)
            {
                var parent = Path.GetDirectoryName(Combine(1, path));
                if (e.ToString().Contains("550") && parent.Trim('\\', '/') != "")
                {
                    if (CreateFtpDir(parent, false))
                    {
                        return CreateFtpDir(path, false);
                    }
                }
            }
            return false;
        }

        public bool ExistFtpPath(string ftpPath)
        {
            var req = createCreateDirRequest(ftpPath + "/");
            req.Method = WebRequestMethods.Ftp.ListDirectory;
            try
            {
                req.GetResponse().Close();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("550"))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
