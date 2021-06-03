using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoItemDetect6._0_C_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 给各个表格加上列名
        private void Initial_columns(object sender, DataGridView tmp)
        {
            DataGridViewTextBoxColumn autorun_entry = new DataGridViewTextBoxColumn();

            DataGridViewTextBoxColumn description = new DataGridViewTextBoxColumn();

            DataGridViewTextBoxColumn publisher = new DataGridViewTextBoxColumn();

            DataGridViewTextBoxColumn image_path = new DataGridViewTextBoxColumn();

            DataGridViewTextBoxColumn timestamp = new DataGridViewTextBoxColumn();

            DataGridViewImageColumn virus_total = new DataGridViewImageColumn();

            autorun_entry.HeaderText = "Autorun Entry";
            description.HeaderText = "Description";
            publisher.HeaderText = "Publisher";
            image_path.HeaderText = "Image Path";
            timestamp.HeaderText = "Timestamp";
            virus_total.HeaderText = "Virus Total";

            tmp.Columns.Add(autorun_entry);
            tmp.Columns.Add(description);
            tmp.Columns.Add(publisher);
            tmp.Columns.Add(image_path);
            tmp.Columns.Add(timestamp);
            tmp.Columns.Add(virus_total);

            return;
        }

        // 处理注册表中获得的路径，让它尽量合理
        private string Process_illegal_path(string target_str)
        {
            string res = null;

            int n = target_str.Length;
            if (n == 0) return res;

            if(target_str[0] == '"')
            {
                for(int i=1; i<n; i++)
                {
                    if(target_str[i] == '"')
                    {
                        res = target_str.Substring(1, i - 1);
                        break;
                    }
                }
            }
            else
            {
                for(int i=1; i<n; i++)
                {
                    if(target_str[i] == '-' && target_str[i-1] == ' ')
                    {
                        res = target_str.Substring(0, i - 1);
                        break;
                    }
                    if(i == (n-1))
                    {
                        res = target_str;
                    }
                }
            }

            return res;
        }

        // 根据文件路径，获取对应的信息
        private void Get_file_information(string path)
        {
            // File提供了很多静态方法，直接调用即可
            // FileInfo也是同一类型，但是方法不是静态
            FileInfo file_process_class = new FileInfo(path);

            // 根据文件路径，获取了一些基本信息
            // 这里一直在处理各种格式信息，我真的是直接醉了
            string name = file_process_class.Name;
            string timestamp = file_process_class.CreationTime.ToShortDateString();
            string ImagePath = path;


            return;
        }
        private void test_of_dataGridView1(object sender, DataGridView target_dataGridView)
        {
            // 以下是一个比较完整的读取注册表的流程
            // 可惜的是，并没有实现完全的前后端分离

            // 想要获取详细信息，需要用shell类，这一点是需要时刻谨记的
            ListViewItem tmp = new ListViewItem(
                new string[] { "path" });

            this.listView1.Items.Add(tmp);

            // 这里涉及到内容提取了

            RegistryKey currentUser = Registry.CurrentUser;
            // 一层层向下打开
            RegistryKey runData = currentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Run");

            // if (runData == null) res += "failing!";

            string[] name_of_auto = runData.GetValueNames();

            // 以下这个键已经被关闭了
            // currentUser.Close();

            string res = "";
            int n = name_of_auto.Length;

            res += n.ToString();

            List<string> name_of_ = new List<string>();
            List<string> val_of_ = new List<string>();

            for (int i = 0; i < n; i++)
            {
                string name_db = name_of_auto[i];
                string val_db = (string)runData.GetValue(name_db);

                val_db = this.Process_illegal_path(val_db);
                res += " ";
                val_of_.Add(val_db);
                name_of_.Add(name_db);
            }

            // 现在已经完成了路径的提取，逻辑较为完善了


            /*
            int index=this.dataGridView1.Rows.Add();

            this.dataGridView1.Rows[index].Cells[0].Value = "1";

            this.dataGridView1.Rows[index].Cells[1].Value = "2";

            this.dataGridView1.Rows[index].Cells[2].Value = "监听";
             */
            // 以后要把这里的循环改成某个类，和表格一一对应是最好的
            // 最好再加上一个简单的类别转换

            foreach (string val in val_of_)
            {
                int index = target_dataGridView.Rows.Add();

                target_dataGridView.Rows[index].Cells[0].Value = val; 
            }
            
            this.listView1.Items.Add("\n" + res);

            this.listView1.Items.Add("\n" + res);

            return;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // 这里初始化控件的列，是需要考虑的

            this.Initial_columns(sender, this.dataGridView1);
            this.Initial_columns(sender, this.dataGridView2);
            this.Initial_columns(sender, this.dataGridView3);
            this.Initial_columns(sender, this.dataGridView4);
            this.Initial_columns(sender, this.dataGridView5);

            this.test_of_dataGridView1(sender, this.dataGridView1);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            
        }

        // C#
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
