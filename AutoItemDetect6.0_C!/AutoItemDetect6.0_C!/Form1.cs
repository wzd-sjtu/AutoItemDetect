using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // 获取第一个路径的所有信息
        private void get_currentuser_autorun()
        {
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

            for (int i = 0; i < n; i++)
            {
                res += runData.GetValue(name_of_auto[i]);
                res += " ";
                res += name_of_auto[i];
            }

            this.listView1.Items.Add("\n" + res);

            return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
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

            for (int i=0; i<n; i++)
            {
                res += runData.GetValue(name_of_auto[i]);
                res += " ";
                res += name_of_auto[i];
            }

            this.listView1.Items.Add("\n" + res);

            return;
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
    }
}
