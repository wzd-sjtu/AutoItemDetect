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
using Shell32;

// 记得加上一个单独的注册表条目，提高程序的可读性

namespace AutoItemDetect6._0_C_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 四个链表
        private List<TargetInformation> LogonList;
        private List<TargetInformation> DriversList;
        private List<TargetInformation> ServiceList;
        private List<TargetInformation> ScheduledList;
        // 给各个表格加上列名
        private class TargetInformation
        {
            public string Name;
            public string Description;
            public string ImagePath;
            public string Publisher;
            public string timeStamp;
            public string size;
            public string owner;
            // 中式编程 版权
            public string banquan;
            public string type;

            public bool isSingleData;
            public TargetInformation() 
            {
                this.Name = " ";
                this.Description = " ";
                this.ImagePath = " ";
                this.Publisher = " ";
                this.timeStamp = " ";
                this.size = " ";
                this.owner = " ";
                this.banquan = " ";
                this.type = " ";
            }
        }

        // 获取注册表下所有键值的路径，被优化过的路径
        // 需要在外面提供主键类+注册表子键具体路径
        private List<string> GetTargetPath(RegistryKey targetKeyType, string registerTablePath)
        {

            List<string> ret = new List<string>();

            // RegistryKey currentUser = Registry.CurrentUser;

            // registerTablePath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey runData = targetKeyType.OpenSubKey(@registerTablePath);

            if (runData == null) return new List<string>();

            string[] name_of_auto = { };
            string[] qq = runData.GetValueNames();

            // 一些空异常处理,这里出现了打开key失败？
            if(qq != null)
            {
                name_of_auto = qq;
            }
           

            int n = name_of_auto.Length;

            List<string> name_of_ = new List<string>();
            List<string> val_of_ = new List<string>();

            for (int i = 0; i < n; i++)
            {
                string name_db = name_of_auto[i];
                string val_db = (string)runData.GetValue(name_db);
                // 处理异常数字
                val_db = this.Process_illegal_path(val_db);
                val_of_.Add(val_db);
                name_of_.Add(name_db);
            }

            // 将键值返回，更加具体的信息需要用shell32库来编写
            ret = val_of_;
            return ret;
        }

        // 传递具体路径，获取解析后的类
        // type是存下的冗余接口
        private TargetInformation GetPathDetailedInfo(string path, string type)
        {

            TargetInformation ret = new TargetInformation();
            

            FileInfo file_process_class = new FileInfo(path);


            Shell32.Shell shell = new Shell32.ShellClass();


            Folder folder = shell.NameSpace(path.Substring(0, path.LastIndexOf('\\')));

            FolderItem item = folder.ParseName(path.Substring(path.LastIndexOf('\\') + 1));

            // 不需要字典传参
            // Dictionary<string, string> Properities = new Dictionary<string, string>();

            int i = 0;

            while (true)
            {
                string key = folder.GetDetailsOf(null, i);
                if (string.IsNullOrEmpty(key))
                {
                    break;
                }
                string value = folder.GetDetailsOf(item, i);

                // 这里需要适当的异常处理

                i++;

                // 填充写好的类
                switch (key)
                {
                    case "名称":
                        {
                            ret.Name = value;
                            break;
                        }
                    case "大小":
                        {
                            ret.size = value;
                            break;
                        }
                    case "创建日期":
                        {
                            ret.timeStamp = value;
                            break;
                        }
                    case "所有者":
                        {
                            ret.owner = value;
                            break;
                        }
                    case "版权":
                        {
                            ret.banquan = value;
                            break;
                        }
                    case "公司":
                        {
                            ret.Publisher = value;
                            break;
                        }
                    case "文件说明":
                        {
                            ret.Description = value;
                            break;
                        }
                    default:
                        break;
                }
                // ret.type = type; //貌似没有必要？
                ret.ImagePath = path;
                ret.isSingleData = true;
            }

            return ret;
        }

        // type是冗余参数，可以任意取值，是没有意义的参数
        private void putDataIntoList(List<string> registerTablePath, List<TargetInformation> target, RegistryKey MainKey, string type)
        {
            
            foreach(string path in registerTablePath)
            {
                // 子健无法完全确定？

                List<string> programPathRes = this.GetTargetPath(MainKey, path);
                TargetInformation highestLevel = new TargetInformation();
                highestLevel.isSingleData = false;

                // 很多别的地方的系统权限注册表无法读取，应当是没有调用对应的接口，导致无法获取
                // 现在整个程序需要向虚拟机转移，从而完成最终的目标
                string tmp_path = "";
                if(MainKey == Registry.LocalMachine)
                {
                    tmp_path = "HKLM//" + path;
                }
                else if(MainKey == Registry.CurrentUser)
                {
                    tmp_path = "HKCU//" + path;
                }

                highestLevel.ImagePath = tmp_path;
                target.Add(highestLevel);

                foreach (string absolutePath in programPathRes)
                {
                    // 一点异常处理
                    if (string.IsNullOrEmpty(absolutePath))
                    {
                        continue;
                    }
                    TargetInformation tmp = this.GetPathDetailedInfo(absolutePath, type);
                    if (tmp != null)
                        target.Add(tmp);
                }

            }
            return;
        }

        // 以下四个函数，就是为了快速填充链表的，效果貌似还可以喽？
        private void GetLogonInfo()
        {
            // 不需要传入参数，内部自带参数
            // 这里会存入很多路径的
            string path1 = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            RegistryKey currentUser = Registry.CurrentUser;
            List<string> list_of_register_key1 = new List<string>();
            list_of_register_key1.Add(path1);

            // 这里完成了公用代码封装，还算是比较合理的
            this.putDataIntoList(list_of_register_key1, this.LogonList, currentUser, "Logon");


            // 下面是另一个注册表类的封装，内容总归是合理的
            string path2 = "SYSTEM\\CurrentControlSet\\Control\\SafeBoot\\AlternateShell";
            string path3 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            string path4 = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run";
            string path5 = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Folders\\Common Startup";
            string path6 = "SOFTWARE\\Microsoft\\Active Setup\\Installed Components";
            string path7 = "SOFTWARE\\Wow6432Node\\Microsoft\\Active Setup\\Installed Components";
            RegistryKey localMachine = Registry.LocalMachine;
            List<string> list_of_register_key2 = new List<string>();

            list_of_register_key2.Add(path2);
            list_of_register_key2.Add(path3);
            list_of_register_key2.Add(path4);
            list_of_register_key2.Add(path5);
            list_of_register_key2.Add(path6);
            list_of_register_key2.Add(path7);

            this.putDataIntoList(list_of_register_key2, this.LogonList, localMachine, "Logon");

            return;
        }
        private void GetDriversInfo()
        {
            return;
        }
        private void GetScheduledInfo()
        {
            return;
        }
        // 直接写drivers有问题？
        private void GetServicesInfo()
        {
            return;
        }

        private void putDataIntoDataGridView(List<TargetInformation> infoList, DataGridView targetDataGridView)
        {
            // 循环遍历，将数据一点点填入这个表格，内容总是清晰地

            // 这种填充函数，其实给了快速处理数据的机会了

            int n = infoList.Count;
            int index = 0;

            
            for(int i=0; i<n; i++)
            {
                TargetInformation tmp = infoList[i];
                index = targetDataGridView.Rows.Add();

                if(tmp != null && tmp.isSingleData)
                {
                    // Autorun Entry
                    targetDataGridView.Rows[index].Cells[0].Value = tmp.Name;
                    // Description
                    targetDataGridView.Rows[index].Cells[1].Value = tmp.Description;
                    // Publisher
                    targetDataGridView.Rows[index].Cells[2].Value = tmp.Publisher;
                    // ImagePath?没有存储
                    targetDataGridView.Rows[index].Cells[3].Value = tmp.ImagePath;
                    // timeStamp 时间戳
                    targetDataGridView.Rows[index].Cells[4].Value = tmp.timeStamp;
                }
                else
                {
                    // Autorun Entry
                    targetDataGridView.Rows[index].Cells[0].Value = tmp.ImagePath;
                }
                


            }

            return;
        }
        private void putDataIntoDataGridView_total(DataGridView targetDataGridView)
        {
            this.putDataIntoDataGridView(this.LogonList, this.dataGridView1);
            this.putDataIntoDataGridView(this.ServiceList, this.dataGridView1);
            this.putDataIntoDataGridView(this.DriversList, this.dataGridView1);
            this.putDataIntoDataGridView(this.ScheduledList, this.dataGridView1);

            return;
        }

        private void InitialAllType()
        {
            // 暂时先不使用多线程开发，先开发一个小函数
            this.GetLogonInfo();
            this.putDataIntoDataGridView(this.LogonList, this.dataGridView2);

            this.GetServicesInfo();
            // this.putDataIntoDataGridView(this.ServiceList, this.dataGridView3);

            this.GetDriversInfo();
            // this.putDataIntoDataGridView(this.DriversList, this.dataGridView4);

            this.GetScheduledInfo();
            // this.putDataIntoDataGridView(this.ScheduledList, this.dataGridView5);



            // 最后还要把最后一列填充进去，逻辑是严谨的
            // this.putDataIntoDataGridView_total(this.dataGridView1);

            // this.putDataIntoDataGridView(this.LogonList, this.dataGridView2);
        }

        // 两个比较简单的处理类，类似于一个小工具
        // 初始化表格列名
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





        // 早期测试写出的类，以后移植时，首先处理这里的类

        // 根据文件路径，获取对应的信息
        private Dictionary<string, string> Get_file_information(string path)
        {


            FileInfo file_process_class = new FileInfo(path);


            Shell32.Shell shell = new Shell32.ShellClass();


            Folder folder = shell.NameSpace(path.Substring(0,path.LastIndexOf('\\')));

            FolderItem item = folder.ParseName(path.Substring(path.LastIndexOf('\\') + 1));

            Dictionary<string, string> Properities = new Dictionary<string, string>();

            int i = 0;

            while(true)
            {
                string key = folder.GetDetailsOf(null, i);
                if(string.IsNullOrEmpty(key))
                {
                    break;
                }
                string value = folder.GetDetailsOf(item, i);

                // 这里需要适当的异常处理
                if(!Properities.ContainsKey(key))
                    Properities.Add(key, value);

                i++;
            }

            // 这个ShellClass的API应当如何使用呢？网上暂时找不到资料？

            return Properities;
        }
        // private test
        private void test_of_dataGridView2(object sender)
        {
            // 在这里的内部，完成所有信息获取
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

            string tset = "";
            for (int i = 0; i < n; i++)
            {
                string name_db = name_of_auto[i];
                string val_db = (string)runData.GetValue(name_db);

                val_db = this.Process_illegal_path(val_db);
                res += " ";
                val_of_.Add(val_db);
                name_of_.Add(name_db);
                if(i == 0)
                    tset = val_db;
            }

            // 我们实验的路径就是tset的信息哦
            // 信息总归是恒定的

            Dictionary<string, string> ret = Get_file_information(tset);
            // Dictionary<string, string> ret = new Dictionary<string, string>();

            foreach (var item in ret)
            {
                int index = this.dataGridView2.Rows.Add();

                this.dataGridView2.Rows[index].Cells[0].Value = item.Key;

                this.dataGridView2.Rows[index].Cells[1].Value = item.Value;

            }

            return;


        }
        // private test code
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
        

        // Form1的初始化设计
        private void Form1_Load(object sender, EventArgs e)
        {
            // 这里初始化控件的列，是需要考虑的
            this.LogonList = new List<TargetInformation>();
            this.ScheduledList = new List<TargetInformation>();
            this.DriversList = new List<TargetInformation>();
            this.ServiceList = new List<TargetInformation>();

            this.Initial_columns(sender, this.dataGridView1);
            this.Initial_columns(sender, this.dataGridView2);
            this.Initial_columns(sender, this.dataGridView3);
            this.Initial_columns(sender, this.dataGridView4);
            this.Initial_columns(sender, this.dataGridView5);

            // 两个较小的测试函数
            // this.test_of_dataGridView1(sender, this.dataGridView1);

            // this.test_of_dataGridView2(sender);

            // 以下这个函数含有整个程序的架构，至此，本程序框架基本搭好了
            this.InitialAllType();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
