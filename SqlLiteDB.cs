using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;
using System.Data;

namespace FtpBack
{
    class SqlLiteDB : IDB
    {

        private string dbPath;

        public void Ini()
        {
            if (!Directory.Exists("db"))
            {
                Directory.CreateDirectory("db");
            }
            dbPath = Path.Combine(Program.EXEPATH + @"db\", WorkName + ".db");
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                using (var conn = createConn())
                {
                    var comm = conn.CreateCommand();
                    comm.CommandText = @"CREATE TABLE [files] (
[id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,
[work_name] VARCHAR(50)  NULL,
[ftp_path] VARCHAR(200)  NULL,
[local_path] VARCHAR(200)  NULL,
[size] FLOAT NULL,
[add_time] TIMESTAMP DEFAULT CURRENT_TIME NULL,
[error] INTEGER DEFAULT '0' NULL,
[error_message] VARCHAR(200)  NULL
);
CREATE TABLE [config] (
[name] VARCHAR(100)  NULL,
[value] VARCHAR(20)  NULL
);
CREATE TABLE [files_success] (
[id] INTEGER  PRIMARY KEY AUTOINCREMENT NOT NULL,
[work_name] VARCHAR(50)  NULL,
[ftp_path] VARCHAR(200)  NULL,
[local_path] VARCHAR(200)  NULL,
[size] FLOAT NULL,
[add_time] TIMESTAMP  NULL,
[up_time] TIMESTAMP  NULL
)
";
                    comm.ExecuteNonQuery();
                }
            }
        }

        private SQLiteConnection createConn()
        {
            var conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", dbPath));
            conn.Open();
            return conn;
        }

        public SqlLiteDB(string workName)
        {
            this.WorkName = workName;
            Ini();
        }

        public bool IsExist(FileInfo file)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("select * from files  where work_name='{0}' and local_path='{1}' and error<2", file.WorkName, file.LocalPath);
                return comm.ExecuteReader().HasRows;
            }
        }

        public bool Add(FileInfo file)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("insert into files(work_name,ftp_path,local_path,size,add_time) values('{0}','{1}','{2}','{3}','{4}')", this.WorkName, file.FtpPath, file.LocalPath, file.Size, file.AddTime.ToString("yyyy-MM-dd HH:mm:ss"));
                return comm.ExecuteNonQuery() > 0;
            }
        }

        public bool Remove(int id)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("delete from files where id='{0}'", id);
                return comm.ExecuteNonQuery() > 0;
            }
        }

        public bool RemoveError()
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("delete from files where error>2 ");
                return comm.ExecuteNonQuery() > 0;
            }
        }

        public bool RemoveAll()
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("delete from files where error=0  ");
                return comm.ExecuteNonQuery() > 0;
            }
        }

        public void ReportUpSuccess(FileInfo file)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("insert into files_success(work_name,ftp_path,local_path,size,add_time,up_time) values('{0}','{1}','{2}','{3}','{4}','{5}')", file.WorkName, file.FtpPath, file.LocalPath, file.Size, file.AddTime.ToString("yyyy-MM-dd HH:mm:ss"), file.UpTime.ToString("yyyy-MM-dd HH:mm:ss"));
                comm.ExecuteNonQuery();
            }
        }


        public string WorkName
        {
            get;
            set;
        }

        public FileInfo GetOne()
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("select id, work_name, ftp_path, local_path,size, add_time from files  where error<3 limit 0,1 ");
                var reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    var file = new FileInfo();
                    file.ID = reader.GetInt32(0);
                    file.WorkName = reader.GetString(1);
                    file.FtpPath = reader.GetString(2);
                    file.LocalPath = reader.GetString(3);
                    file.Size = reader.GetDouble(4);
                    file.AddTime = reader.GetDateTime(5);
                    return file;
                }
                else
                {
                    return null;
                }
            }
        }

        public void ReportError(int id, string error_message)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("update files set error=error+1 ,error_message='{1}' where id='{0}'", id, error_message);
                comm.ExecuteNonQuery();
            }
        }

        public void ReplyUp(int id)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("update files set error=0 ,error_message='' where id='{0}'", id);
                comm.ExecuteNonQuery();
            }
        }


        public void Set(string name, string value)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("delete from config where name='{0}'", name);
                comm.ExecuteNonQuery();
                comm.CommandText = String.Format("insert into config(name,value) values('{0}','{1}')", name, value);
                comm.ExecuteNonQuery();
            }
        }

        public string Get(string name)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("select value from config where name='{0}'", name);
                var reader = comm.ExecuteReader();
                if (reader.Read())
                {
                    return reader[0].ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public DataTable GetTable(string sql)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = sql;
                var reader = comm.ExecuteReader();
                var table = new DataTable();
                table.Load(reader);
                return table;

            }
        }

        public bool RemoveSuccess(int id)
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("delete from files_success where id='{0}'", id);
                return comm.ExecuteNonQuery() > 0;
            }
        }

        public bool RemoveSuccessAll()
        {
            using (var conn = createConn())
            {
                var comm = conn.CreateCommand();
                comm.CommandText = String.Format("delete from files_success  ");
                return comm.ExecuteNonQuery() > 0;
            }
        }

    }
}
