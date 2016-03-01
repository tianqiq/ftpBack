using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FtpBack
{
    interface IDB
    {
        bool IsExist(FileInfo file);

        bool Add(FileInfo file);

        bool Remove(int id);

        bool RemoveAll();

        bool RemoveError();

        FileInfo GetOne();

        void ReportError(int id, string error_message);

        void Set(string name, string value);

        string Get(string name);

        void ReportUpSuccess(FileInfo file);

        void ReplyUp(int id);

        bool RemoveSuccess(int id);

        bool RemoveSuccessAll();
    }
}
