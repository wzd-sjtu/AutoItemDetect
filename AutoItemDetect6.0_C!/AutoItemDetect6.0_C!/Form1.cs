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
using TaskScheduler;
using System.Threading;
using System.Text.RegularExpressions;

// 记得加上一个单独的注册表条目，提高程序的可读性
// 专门用于输出的函数
// int index = this.dataGridView1.Rows.Add();
// this.dataGridView1.Rows[index].Cells[0].Value = tmp_path;
namespace AutoItemDetect6._0_C_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 四个链表
        private delegate void MyDelegate(List<TargetInformation> infoList, DataGridView targetDataGridView);

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
        private List<string> GetTargetPath(RegistryKey targetKeyType, string registerTablePath, string type)
        {

            // 还没有生成type，type需要从高层传递到底层，这样才是较为合理的

            List<string> ret = new List<string>();

            // RegistryKey currentUser = Registry.CurrentUser;

            // registerTablePath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
            // 查询run当然可以直接对应路径，但是查询别的是不成立的，会出现各种问题
            // 我裂开，底层出问题？
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

            // 居然默认为都是path？那肯定会出问题的
            // 直接修改底层函数，提高效率


            // "Service""Scheduled Task""Drive"

            if (type == "Logon")
            {
                for (int i = 0; i < n; i++)
                {
                    // 光是getValue了，肯定会出现无法预知的错误

                    string name_db = name_of_auto[i];
                    string val_db = (string)runData.GetValue(name_db).ToString();
                    // byte[] reg_binary = { };

                    // 处理异常数字
                    val_db = this.Process_illegal_path(val_db);
                    val_of_.Add(val_db);
                    name_of_.Add(name_db);
                }

                // 将键值返回，更加具体的信息需要用shell32库来编写
                ret = val_of_;
            }
            // 以下是几个基本类的处理
            else if(type == "Service")
            {
                // 差不多算是写完了Service的内容了，就是加载稍微有点慢，希望加一些按钮进去？
                // 需要遍历键下面的所有键
                string[] childKey = runData.GetSubKeyNames();
                if (childKey == null) 
                {
                    childKey = new string[0];
                }
                foreach(string child in childKey)
                {
                    RegistryKey lowerRunData = runData.OpenSubKey(@child);
                    string[] name_of_child = { };
                    string[] wshLove = lowerRunData.GetValueNames();
                    if(wshLove != null)
                    {
                        name_of_child = wshLove;
                    }
                    // 第一重筛选，还有第二重筛选，真的头疼哦
                    if(name_of_child.Contains("ImagePath"))
                    {
                        // 暂时说明不是废弃的内容
                        // 之后需要区分drivers和services
                        string tmp_path = (string)lowerRunData.GetValue("ImagePath").ToString();
                        tmp_path = this.Process_illegal_path(tmp_path);

                        int tmp_path_size = tmp_path.Length;
                        if (tmp_path.Contains("svchost.exe")) // 说明是exe，并且位于svchost里面
                        {
                            /*
                            if(name_of_child.Contains("ServiceDll"))
                            {
                                // 表明是正儿八经的服务
                                string significantPath = (string)lowerRunData.GetValue("ServiceDll").ToString();
                                ret.Add(significantPath);
                                // 至此，成功得到所有的path，是合理的吗？
                            }
                            */
                            if(tmp_path.Contains("Description"))
                            {
                                // 存储目标dll的文件路径
                                string zyhLove = (string)lowerRunData.GetValue("Description").ToString();
                                zyhLove = this.Process_illegal_path_svchost(zyhLove);

                                // 从此存入了目标目录
                                ret.Add(zyhLove);
                            }
                            
                        }
                        else if(tmp_path[tmp_path_size-1] == 'e') // 正常的exe
                        {
                            ret.Add(tmp_path);
                        }
                        // else if(tmp_path[tmp_path_size - 1] == 's') // 说明是sys
                    }


                }
            }
            else if (type == "Driver")
            {
                // 下面我们来处理Driver的过程？
                // Drivers文件的统一存储位置
                // C:\Windows\System32\drivers

                string[] childKey = runData.GetSubKeyNames();
                if (childKey == null)
                {
                    childKey = new string[0];
                }
                foreach (string child in childKey)
                {
                    RegistryKey lowerRunData = runData.OpenSubKey(@child);
                    string[] name_of_child = { };
                    string[] wshLove = lowerRunData.GetValueNames();
                    if (wshLove != null)
                    {
                        name_of_child = wshLove;
                    }
                    // 第一重筛选，还有第二重筛选，真的头疼哦
                    if (name_of_child.Contains("ImagePath"))
                    {
                        // 暂时说明不是废弃的内容
                        string tmp_path = (string)lowerRunData.GetValue("ImagePath").ToString();
                        tmp_path = this.Process_illegal_path(tmp_path);

                        // 专门用于输出的函数

                        // int tmp_path_size = tmp_path.Length;
                        if(tmp_path.Contains(".sys") || tmp_path.Contains(".SYS"))
                        {
                            if(tmp_path.Contains("\\??\\"))
                            {
                                // 需要直接去掉部分内容？
                                // 4采用的是逻辑分析
                                // \??\C:\WINDOWS\system32\drivers\360Sensor64.sys
                                tmp_path = tmp_path.Replace("\\??\\", "");
                            }
                            else if(tmp_path.Contains("SystemRoot"))
                            {
                                // 需要把Root转换一下
                                // tmp_path = tmp_path.Replace("SystemRoot", "@%SystemRoot%");
                                tmp_path = tmp_path.Replace("SystemRoot", "C:\\WINDOWS");
                            }
                            else
                            {
                                tmp_path = "C:\\WINDOWS\\" + tmp_path;
                            }
                            // 去掉头，在加入链表之前，就需要将头部不合法字段去掉
                            int be_loc = 0;
                            while (be_loc < tmp_path.Length && tmp_path[be_loc] == '\\') be_loc++;
                            tmp_path = tmp_path.Substring(be_loc);
                            ret.Add(tmp_path);
                            // 无法批量处理，只能一点点处理了
                            // 而且内容还
                        }
                    }
                }
            }

            else if(type == "Scheduled Task")
            {
                string[] childKey = runData.GetSubKeyNames();
                if (childKey == null)
                {
                    childKey = new string[0];
                }
                foreach (string child in childKey)
                {
                    bool legal_path = false;
                    RegistryKey lowerRunData = runData.OpenSubKey(@child);
                    string[] name_of_child = { };
                    string[] wshLove = lowerRunData.GetValueNames();
                    if (wshLove != null)
                    {
                        name_of_child = wshLove;
                    }
                    // 仅仅筛选Actions是绝对不成立的

                    string tmp_path = ""; // 首先初始化一个初始值
                    string SystemRootReplace = "C:\\windows";
                    string ProgramFilesReplace = "C:\\Program Files";
                    string windirReplace = "C:\\windows";
                    string systemRootReplace = "C:\\windows";

                    // 这里进行三重嵌套即可，就是不太优美
                    if (tmp_path.Length == 0 && name_of_child.Contains("Author"))
                    {
                        tmp_path = (string)lowerRunData.GetValue("Author").ToString();
                        
                        if(tmp_path != null && tmp_path.Length>0 && tmp_path[0] == '$')
                        {
                            // 证明有路径
                            int begin_loc = tmp_path.IndexOf('@');
                            int end_loc = tmp_path.IndexOf(',');
                            tmp_path = tmp_path.Substring(begin_loc + 1, end_loc - begin_loc - 1);
                            legal_path = true;
                        }
                    }
                    else if (tmp_path.Length == 0 && name_of_child.Contains("Description"))
                    {
                        tmp_path = (string)lowerRunData.GetValue("Description").ToString();

                        if (tmp_path != null && tmp_path.Length > 0 && tmp_path[0] == '$')
                        {
                            // 证明有路径
                            int begin_loc = tmp_path.IndexOf('@');
                            int end_loc = tmp_path.IndexOf(',');
                            tmp_path = tmp_path.Substring(begin_loc + 1, end_loc - begin_loc - 1);
                            legal_path = true;
                        }
                        
                    }

                    else if (tmp_path.Length == 0 && name_of_child.Contains("Source"))
                    {
                        tmp_path = (string)lowerRunData.GetValue("Source").ToString();

                        if (tmp_path != null && tmp_path.Length > 0 && tmp_path[0] == '$')
                        {
                            // 证明有路径
                            int begin_loc = tmp_path.IndexOf('@');
                            int end_loc = tmp_path.IndexOf(',');
                            tmp_path = tmp_path.Substring(begin_loc + 1, end_loc - begin_loc - 1);
                            legal_path = true;
                        }
                    }
                    // 一定有Actions的,这里面可能会有别的信息的
                    else if (tmp_path.Length == 0 && name_of_child.Contains("Actions"))
                    {
                        // 暂时说明不是废弃的内容
                        byte[] bytesMessage = (byte[])lowerRunData.GetValue("Actions");

                        tmp_path = System.Text.Encoding.UTF8.GetString(bytesMessage);
                        tmp_path = tmp_path.Replace("\0", "");
                        // 目标程序正则表达式，考虑并不完备，可能会出问题
                        string pat = @"%(?:(?!%)(?:.|\n))*%(?:(?!\.exe)(?:.|\n))*\.exe";

                        Regex regex = new Regex(pat);

                        if(regex.IsMatch(tmp_path))
                        {
                            tmp_path = (string)regex.Match(tmp_path).Value;
                            
                            legal_path = true;

                            // 需要对变量进行额外处理
                        }
                    }
                    if(legal_path)
                    {
                        // 每个dir都要处理一下
                        
                        tmp_path = tmp_path.Replace("%SystemRoot%", systemRootReplace);
                        tmp_path = tmp_path.Replace("%systemRoot%", systemRootReplace);
                        tmp_path = tmp_path.Replace("%systemroot%", systemRootReplace);
                        tmp_path = tmp_path.Replace("%windir%", windirReplace);
                        tmp_path = tmp_path.Replace("%WinDir%", windirReplace);
                        tmp_path = tmp_path.Replace("%winDir%", windirReplace);
                        tmp_path = tmp_path.Replace("ProgramFiles", ProgramFilesReplace);

                        // int index = this.dataGridView1.Rows.Add();
                        // this.dataGridView1.Rows[index].Cells[0].Value = tmp_path;
                        ret.Add(tmp_path);
                    }
                    
                }
            }
            
            

            return ret;
        }

        // 传递具体路径，获取解析后的类
        // type是存下的冗余接口
        private TargetInformation GetPathDetailedInfo(string path, string type)
        {
            if (path == null) return new TargetInformation();

            TargetInformation ret = new TargetInformation();
            

            FileInfo file_process_class = new FileInfo(path);


            Shell32.Shell shell = new Shell32.ShellClass();

            // int index = this.dataGridView1.Rows.Add();
            // this.dataGridView1.Rows[index].Cells[0].Value = path;

            // 需要使用别打的权限
            // 典型的权限不够，无法查看driver的详细信息
            Folder folder = shell.NameSpace(path.Substring(0, path.LastIndexOf('\\')));
            if (folder is null)
            {
                // 不合法的路径需要展示出来
                // int ii = this.dataGridView1.Rows.Add();
                // this.dataGridView1.Rows[ii].Cells[0].Value = "WRONG FOLDER!  " + path.Substring(0, path.LastIndexOf('\\'));

                return ret;
            }
            else
            {
                // int iii = this.dataGridView1.Rows.Add();
                // this.dataGridView1.Rows[iii].Cells[0].Value = "YES FOLDER!  " + path.Substring(0, path.LastIndexOf('\\'));
            }
            FolderItem item = folder.ParseName(path.Substring(path.LastIndexOf('\\') + 1));
            if (item is null) return ret;
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

                List<string> programPathRes = this.GetTargetPath(MainKey, path, type);
                TargetInformation highestLevel = new TargetInformation();
                highestLevel.isSingleData = false;

                // 很多别的地方的系统权限注册表无法读取，应当是没有调用对应的接口，导致无法获取
                // 现在整个程序需要向虚拟机转移，从而完成最终的目标
                string tmp_path = "";
                if(MainKey == Registry.LocalMachine)
                {
                    tmp_path = "HKLM\\" + path;
                }
                else if(MainKey == Registry.CurrentUser)
                {
                    tmp_path = "HKCU\\" + path;
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
        // 刚刚处理完Logon类，这个类别算是完整的
        private void GetLogonInfo()
        {
            // 和这个链表绑定？
            

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

            // 取出数据
            this.putDataIntoList(list_of_register_key2, this.LogonList, localMachine, "Logon");
            return;
        }
        private void GetDriversInfo()
        {
            // 读取位置和GetServicesInfo完全相似

            string path1 = "System\\CurrentControlSet\\Services";
            RegistryKey localMachine = Registry.LocalMachine;
            List<string> list_of_register_key1 = new List<string>();
            list_of_register_key1.Add(path1);

            // 这里完成了公用代码封装，还算是比较合理的

            // 这里是包装类的区别，也就是包装类区别的原因
            this.putDataIntoList(list_of_register_key1, this.DriversList, localMachine, "Driver");

            return;
        }

        // 这里使用的是操作计划任务的位置，实际上读取注册表是更加合理的
        private IRegisteredTaskCollection GetAllTasks()
        {
            TaskSchedulerClass ts = new TaskSchedulerClass();
            ts.Connect(null, null, null, null);
            ITaskFolder folder = ts.GetFolder("\\");
            IRegisteredTaskCollection task_exists = folder.GetTasks(1);


            for(int i=1; i<=task_exists.Count; i++)
            {
                IRegisteredTask t = task_exists[i];

                /*
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
                */

                string description = t.Definition.Data;
                string name = t.Name;
                string ImagePath = t.Path;
                // 更加详细的信息，需要通过path存取


                int index = this.dataGridView1.Rows.Add();

                this.dataGridView1.Rows[index].Cells[0].Value = name;

                this.dataGridView1.Rows[index].Cells[1].Value = ImagePath;

                this.dataGridView1.Rows[index].Cells[2].Value = description;

                // 发现这里的path还需要一些别的定位信息，我直接裂开
                // 现在要处理这些信息？

            }
            return task_exists;
        }
        private void GetScheduledInfo()
        {
            // 这里是完全不同的API接口，谨记

            // 首先测试一下这个多余的类
            // 这个是别的API接口，会把信息返回到主listview上
            // 感觉不太好用
            // this.GetAllTasks();

            /*
             HKLM\Software\Microsoft\Windows NT\CurrentVersion\Schedule\Taskcache\Tasks
             HKLM\Software\Microsoft\Windows NT\CurrentVersion\Schedule\Taskcache\Tree
             */
            string path1 = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Schedule\\Taskcache\\Tasks";
            // Tree一会再说
            string path2 = "Software\\Microsoft\\Windows NT\\CurrentVersion\\Schedule\\Taskcache\\Tree";
            RegistryKey localMachine = Registry.LocalMachine;
            List<string> list_of_register_key1 = new List<string>();

            // HKLM还是Local Machine
            list_of_register_key1.Add(path1);
            list_of_register_key1.Add(path2);

            // 这里完成了公用代码封装，还算是比较合理的

            // 这里是包装类的区别，也就是包装类区别的原因
            this.putDataIntoList(list_of_register_key1, this.ScheduledList, localMachine, "Scheduled Task");

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
                // targetDataGridView.Rows[index].Cells[0].Value = tmp.Name;

                if (tmp != null && tmp.isSingleData)
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

            // 核心就是这个数据处理冲突了
            this.GetLogonInfo();
            this.putDataIntoDataGridView(this.LogonList, this.dataGridView2);

            // Services确实经过了处理，但就是不太对劲
            this.GetServicesInfo();
            this.putDataIntoDataGridView(this.ServiceList, this.dataGridView3);

            this.GetDriversInfo();
            this.putDataIntoDataGridView(this.DriversList, this.dataGridView4);

            this.GetScheduledInfo();
            this.putDataIntoDataGridView(this.ScheduledList, this.dataGridView5);

            // 最后还要把最后一列填充进去
            this.putDataIntoDataGridView_total(this.dataGridView1);

            // this.putDataIntoDataGridView(this.LogonList, this.dataGridView2);
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

            // 关闭非法警告？ 多线程写失败了
            // Control.CheckForIllegalCrossThreadCalls = false;
            this.InitialAllType();
        }



        // 三个比较简单的处理类，类似于一个小工具
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
        private string Process_illegal_path_svchost(string target_str)
        {
            string res = null;

            // 逻辑要清楚，开头是@，用第一个逗号分隔
            // 不行再说
            int n = target_str.Length;

            int i = 0;
            for (i = 1; i < n; i++)
            {
                if (target_str[i] == ',') break;
            }
            res = target_str.Substring(1, i - 1);
            return res;
        }
        private string Process_illegal_path(string target_str)
        {
            string res = null;

            int n = target_str.Length;
            if (n == 0) return res;

            if (target_str[0] == '"')
            {
                for (int i = 1; i < n; i++)
                {
                    if (target_str[i] == '"')
                    {
                        res = target_str.Substring(1, i - 1);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 1; i < n; i++)
                {
                    if (target_str[i] == '-' && target_str[i - 1] == ' ')
                    {
                        res = target_str.Substring(0, i - 1);
                        break;
                    }
                    if (i == (n - 1))
                    {
                        res = target_str;
                    }
                }
            }

            return res;
        }

        // 三个多余的动作
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // 用于测试的按钮
        private void button1_Click(object sender, EventArgs e)
        {
            // C:\WINDOWS\\system32\\drivers\\lltdio.sys
            // C:\WINDOWS\\System32\\drivers\\lsi_sas3i.sys
            // 这个是需要统一的格式，不能加入不合理的东西？
            string path = "C:\\WINDOWS\\System32\\drivers\\lltdio.sys";
            FileInfo file_process_class = new FileInfo(path);


            Shell32.Shell shell = new Shell32.ShellClass();

            // int index = this.dataGridView1.Rows.Add();
            // this.dataGridView1.Rows[index].Cells[0].Value = path;

            // 需要使用别打的权限
            // 典型的权限不够，无法查看driver的详细信息
            Folder folder = shell.NameSpace(path.Substring(0, path.LastIndexOf('\\')));
            if (folder is null)
            {
                int ii = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[ii].Cells[0].Value = "WRONG FOLDER!  " + path.Substring(0, path.LastIndexOf('\\'));
                return;
            }
            else
            {

                int iii = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[iii].Cells[0].Value = "YES FOLDER!  " + path.Substring(0, path.LastIndexOf('\\'));
            }
            FolderItem item = folder.ParseName(path.Substring(path.LastIndexOf('\\') + 1));
            if (item is null) return;
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
                this.listView1.Items.Add(value);
                i++;
            }
        }




        // 以下函数可以忽略，开发流程中的中间代码
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
                if (i == 0)
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

        // 根据文件路径，获取对应的信息
        private Dictionary<string, string> Get_file_information(string path)
        {


            FileInfo file_process_class = new FileInfo(path);


            Shell32.Shell shell = new Shell32.ShellClass();


            Folder folder = shell.NameSpace(path.Substring(0, path.LastIndexOf('\\')));

            FolderItem item = folder.ParseName(path.Substring(path.LastIndexOf('\\') + 1));

            Dictionary<string, string> Properities = new Dictionary<string, string>();

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
                if (!Properities.ContainsKey(key))
                    Properities.Add(key, value);

                i++;
            }

            // 这个ShellClass的API应当如何使用呢？网上暂时找不到资料？

            return Properities;
        }


        private void GetServicesInfo()
        {
            // 获取service的信息喽

            // HKLM\System\CurrentControlSet\Services				2021/6/4 17:48	
            // 很显然，services类禁止直接读取
            string path1 = "System\\CurrentControlSet\\Services";
            RegistryKey localMachine = Registry.LocalMachine;
            List<string> list_of_register_key1 = new List<string>();
            list_of_register_key1.Add(path1);

            // 这里完成了公用代码封装，还算是比较合理的

            // 这里是包装类的区别，也就是包装类区别的原因
            this.putDataIntoList(list_of_register_key1, this.ServiceList, localMachine, "Service");

            return;
        }
    }
}
