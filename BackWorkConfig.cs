using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;
using System.IO;

namespace FtpBack
{
    public class BackWorkConfig
    {
        public String name { get; set; }

        private string _dir;

        public string dir
        {
            get { return _dir; }
            set { _dir = value; }
        }

        private String _fiter = "*.*";

        public String fiter
        {
            get { return _fiter; }
            set { _fiter = value; }
        }

        public FtpConfig ftp { get; set; }

        private bool _run = true;

        public bool run
        {
            get { return _run; }
            set { _run = value; }
        }

        public string fullDir { get; set; }

        public Dictionary<string, BackWorkChildConfig> childs { get; set; }

        public static List<BackWorkConfig> Create(string filePath)
        {
            return JsonMapper.ToObject<List<BackWorkConfig>>(File.ReadAllText(filePath));
        }

        public override string ToString()
        {
            return name.ToString();
        }
    }


    public class BackWorkChildConfig : BackWorkConfig
    {
        public BackWorkConfig Paren { get; set; }

        public string dirName { get; set; }

        public override string ToString()
        {
            return name.ToString();
        }
    }

    public class FtpConfig
    {

        public String host { get; set; }

        public String name { get; set; }

        public String pwd { get; set; }


        private bool _pasv = true;

        public bool pasv
        {
            get { return _pasv; }
            set { _pasv = value; }
        }

        private string _dir = "";

        public string dir
        {
            get { return _dir; }
            set { _dir = value; }
        }

    }


}
