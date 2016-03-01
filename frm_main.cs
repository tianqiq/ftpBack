using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net;

namespace FtpBack
{
    public partial class frm_main : Form
    {

        public class WorkItem
        {
            private BackWorkConfig config;
            public WorkItem(BackWorkConfig config)
            {
                this.config = config;
            }

            public override string ToString()
            {
                if (config is BackWorkChildConfig)
                {
                    return "└" + config.ToString();
                }
                else
                {
                    return config.ToString();
                }
            }

            public BackWorkConfig GetData()
            {
                return config;
            }

            public string GetDBName()
            {
                if (config is BackWorkChildConfig)
                {
                    return (config as BackWorkChildConfig).Paren.name + "(" + (config as BackWorkChildConfig).dirName + ")";
                }
                else
                {
                    return config.name;
                }
            }

        }

        public frm_main()
        {
            InitializeComponent();
        }

        //private FtpWatch watch = new FtpWatch();

        List<BackWorkConfig> config;

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersWidth = 60;
            config = BackWorkConfig.Create(Program.EXEPATH + "/work.json");
            foreach (var item in config)
            {
                comboBox1.Items.Add(new WorkItem(item));
                if (item.childs != null)
                {
                    foreach (var item1 in item.childs)
                    {
                        item1.Value.Paren = item;
                        item1.Value.dirName = item1.Key;
                        item1.Value.dir = item.dir + "/" + item1.Key;
                        comboBox1.Items.Add(new WorkItem(item1.Value));
                    }
                }
            }
            comboBox1.SelectedIndex = comboBox1.Items.Count > 1 ? 1 : 0;
            //watch.Start();
            LoadData();
        }



        private void frm_main_FormClosed(object sender, FormClosedEventArgs e)
        {
           // watch.Stop();
        }

        private string GetTableName()
        {
            return radioButton4.Checked ? "files_success" : "files";
        }

        private string GetSelectRad()
        {
            if (radioButton1.Checked)
            {
                return "未完成";
            }
            else if (radioButton2.Checked)
            {
                return "错误";
            }
            else
            {
                return "已完成";
            }
        }

        IDB db;
        private void button1_Click(object sender, EventArgs e)
        {
            LoadData();
        }




        private void LoadData()
        {
            if (comboBox1.SelectedIndex != 0)
            {
                var workItem = comboBox1.SelectedItem as WorkItem;
                db = new SqlLiteDB(workItem.GetDBName());
                var sql = "select * from {0} {1} ";
                var data = (db as SqlLiteDB).GetTable(string.Format(sql, GetTableName(), buildSql()));
                dataGridView1.Rows.Clear();
                foreach (DataRow item in data.Rows)
                {
                    dataGridView1.Rows.Add(item.ItemArray);
                }
                label2.Text = String.Format("（{0}）", GetSelectRad()) + (data.Rows.Count == 0 ? "无数据" : "总计：" + data.Rows.Count + "条数据");
            }
            else
            {
                MessageBox.Show("请先选择任务！   ", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string buildSql()
        {
            StringBuilder sb = new StringBuilder(" where 1=1 ");
            if (this.radioButton1.Checked)
            {
                sb.AppendFormat(string.Format(" and error=0 "));
            }
            else if (this.radioButton2.Checked)
            {
                sb.AppendFormat(string.Format(" and error>2 "));
            }
            else
            {
                sb.Append(" order by id desc ");
            }
            return sb.ToString();
        }

        private void 重新上传ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                var id = row.Cells[0].Value.ToString();
                db.ReplyUp(Int32.Parse(id));
                dataGridView1.Rows.Remove(row);
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要删除选中的吗？删除后将不能恢复！   ", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                var id = row.Cells[0].Value.ToString();
                if (radioButton4.Checked)
                {
                    db.RemoveSuccess(Int32.Parse(id));
                }
                else
                {
                    db.Remove(Int32.Parse(id));
                }

                dataGridView1.Rows.Remove(row);
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            dataGridView1.Columns["col_error_message"].Visible = 重新上传ToolStripMenuItem.Visible = !radioButton4.Checked;
            if (radioButton4.Checked)
            {
                dataGridView1.Columns["col_error"].HeaderText = "上传时间";
                dataGridView1.Columns["col_error"].Width = 130;
                LoadData();
            }
            else
            {
                dataGridView1.Columns["col_error"].HeaderText = "错误";
                dataGridView1.Columns["col_error"].Width = 40;
            }
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空所有记录吗？清空后将不能恢复！   ", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
            {
                if (radioButton4.Checked)
                {
                    db.RemoveSuccessAll();
                }
                else if (radioButton2.Checked)
                {
                    db.RemoveError();
                }
                else if (radioButton1.Checked)
                {
                    db.RemoveAll();

                }
                dataGridView1.Rows.Clear();
            }
        }



        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                dataGridView1.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dataGridView1.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                LoadData();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            重新上传ToolStripMenuItem.Visible = radioButton2.Checked;
            if ((sender as RadioButton).Checked)
            {
                LoadData();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
            {
                LoadData();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var workItem = comboBox1.SelectedItem as WorkItem;
            if (workItem != null)
            {
                var info = workItem.GetData();
                if (info.ftp != null)
                {
                    textBox1.Text = string.Format("监听目录:{0}，状态:{1},Ftp地址:{2}，Ftp用户名:{3}，Ftp密码:{4}", info.dir, info.run ? "运行" : "停止", info.ftp.host + "/" + info.ftp.dir, info.ftp.name, info.ftp.pwd);
                }
                else
                {
                    textBox1.Text = string.Format("监听目录:{0}，状态:{1}，没有配置ftp信息", info.dir, info.run ? "运行" : "停止");
                }
                LoadData();
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex > 0)
            {
                try
                {
                    System.Diagnostics.Process.Start((comboBox1.SelectedItem as WorkItem).GetData().dir);
                }
                catch (Exception e1)
                {
                    var path = (comboBox1.SelectedItem as WorkItem).GetData().dir;
                    MessageBox.Show(path + "\r\n\r\n" + e1.Message + "    ", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("请先选择任务！   ", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex > -1)
            {
                try
                {
                    var value = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
                    var info = (comboBox1.SelectedItem as WorkItem).GetData();
                    var host = info.ftp.host;
                    System.Diagnostics.Process.Start("http://" + host + "/" + value);
                }
                catch { }
            }
            else if (e.ColumnIndex == 3 && e.RowIndex > -1)
            {
                var value = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                try
                {
                    System.Diagnostics.Process.Start(value);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(value + "\r\n\r\n" + e1.Message + "    ", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }






    }
}
