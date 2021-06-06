# AutoItemDetect

Homework for IS405

发现C++写windows桌面程序好麻烦，配环境太难受了，QT+Windows库需要配到一起

只好用C#来写了，使用visual studio 自带的.Net框架，自带界面

下面是一部分运行截图


![image](https://user-images.githubusercontent.com/46391254/120778259-b63a4c80-c558-11eb-81da-7533c5bc8938.png)

当然，这只是暂时效果，还没有写完。


下面给出简单的实验报告：

根据报告要求，我们主要研究四大类：

Logon：启动目录，基于注册表启动

Services：系统服务

Drivers：系统驱动程序

Scheduled Tasks：计划任务

[toc]

下面对这四个类别分别分析：

# 0.预备知识

在看之前，首先了解一下一些注册表变量：

```
%SystemDrive%                                                             系统安装的磁盘分区
%SystemRoot% = %Windir% WINDODWS                系统目录
%ProgramFiles%　                                                        应用程序默认安装目录
%AppData%                                                                    应用程序数据目录
%CommonProgramFiles%                                            公用文件目录
%HomePath%                                                                 当前活动用户目录
%Temp% =%Tmp%                                                        当前活动用户临时目录
%DriveLetter%                                                                逻辑驱动器分区
%HomeDrive%                                                               当前用户系统所在分区
```

查看常用变量的值：（倘若注册表没有被修改过，我们认为这些变量值不会变化）

```
%USERPROFILE% =C:\Users\用户名
%SystemRoot% =C:\WINDOWS
%SystemDrive% =C:
%APPDATA% =C:\Users\用户名\AppData\Roaming
%LOCALAPPDATA% =C:\Users\用户名\AppData\Local
%windir% =C:\WINDOWS
%Path% =C:\Windows\system32;C:\Windows; 
%ProgramData% =C:\ProgramData
%ProgramFiles% =C:\Program Files
%ProgramFiles(x86)% =C:\Program Files (x86)
```
# 1.Logon 启动目录
## 技术原理
Logon类主要有两个文件夹和几个注册表键来控制控制程序的自启动。

对于文件夹，一般是系统盘中的某个目录，下面存有希望自启动程序的快捷方式或者实体。
对于注册表键，一般是位于注册表的某个键下面，存储有键值，键值就是希望自启动的文件路径。

## 实现细节

###（1）首先来看一下文件夹：
```
%USERPROFILE%\AppData\Roaming\Microsoft\Windows\StartMenu\Programs\Startup
// 转换后即为以下地址（针对Administrator用户）
C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
%ProgramData%\Microsoft\Windows\StartMenu\Programs\Startup
// 转换后为以下地址（针对所有用户）
C:\ProgramData\Microsoft\Windows\StartMenu\Programs\Startup
```

再来看注册表键的情况
查阅资料，我们了解到很多注册表键，下面一个一个来解析：
### （2）USERINIT注册表键值
注册表键为：
```
HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon
```
内容为这个键下方的Userinit变量，

默认为userinit.exe，可用逗号分隔，从而放入多个程序。

以下是它的截图：（路径是定死的）
![image](https://user-images.githubusercontent.com/46391254/120929313-2d1f4300-c71b-11eb-89ab-58fe32536edb.png)

### （3）EXPLORE\RUN键
注册键为：
```
HKEY_CURRENT_USER\\Software\\Microsoft\Windows\\CurrentVersion\\Policies\\Explorer\\Run
HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run
```

计算机太新了，导致这里没有键值，倘若有，直接遍历路径即可。
### （4）RunServiceOnce键
路径为：
```
HKEY_CURRENT_USER\\Softvvare\\Mcrosoft\\Windows\\CurrentVersion\\RunServicesOnce
HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunServicesOnce
```
在Windows10系统中没有发现这个键值
### （5）RunServices键
同样也没有发现这两个键
```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunServices
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunServices
```
### （6）RunOnce
一共有四个键
```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce\Setup
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce\Setup
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce
HEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce
```
他们具体长这个样子
![image](https://user-images.githubusercontent.com/46391254/120929322-390b0500-c71b-11eb-9939-3ddea3ce5678.png)

以下是一个典型的路径：
C:\Program Files\VMware\VMware Tools\vmtoolsd.exe" -n vmusr

### （7）Run键
还是有两个键值
```
HKEY_CURRENT_USER\\Softvvare\\Microsoft\\Windows\\CurrentVersion\\Run
HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run
```

## 隐蔽性状况

以上是对Logon类的自启动分析，当操作系统启动后，会逐一扫描这些项目，并且把对应的程序运行起来。

路径都是定死的，其中的启动项都是绝对路径，明文可读，隐蔽性不好，只要扫描全面，很容易被发现。

还有一个注册表重定向的问题，重定向表如下：

![image](https://user-images.githubusercontent.com/46391254/120929327-41fbd680-c71b-11eb-8b0c-2f9be6b8c1ef.png)


这是在编写程序，指定注册表路径时，可能会发生的情况。

# 2.Services 系统服务

## 技术原理及实现细节

### （1）注册表键

服务程序比Logon简洁很多，所有配置文件位于下面的注册表键下：
```
HKLM\System\CurrentControlSet\Services
```
在这个键下面，还有很多子健，每一个子健都有自己的内部属性值。
下面给出对应的截图：

![image](https://user-images.githubusercontent.com/46391254/120929682-918ed200-c71c-11eb-872e-bdb0b158739c.png)


可以看到，这里有许多键值，代表了不同信息。

### （2）start位标记自启动

可以根据注册表中start取值，得到不同的服务类型

倘若发现start=2，这就意味着这个程序是自启动程序。

### （3）services服务源文件路径分析

对所有内容进行分析，我们得到一些信息如下：

![image](https://user-images.githubusercontent.com/46391254/120929342-4cb66b80-c71b-11eb-8401-6c1d4471dcdb.png)


对于所有的子健，内部元素有以下特征：

统一都有ImagePath字段，只要有ImagePath字段，就有start字段

我们根据ImagePath键+start首先筛选出自启动的可能服务程序
之后再分析：

ImagePath有两种情况：

1.svchost.exe结尾，实际的dll并不在本地，一般会存储在Description字段，前面有一个@，最后用逗号分隔

2.直接目标exe结尾 可以直接加入

3.以*.sys结尾的，是系统驱动文件，不需要在这里分析

### （4）获取对应路径的详细信息

某些dll文件被保护，无法被写入。但是可以读取其中的文件的详细信息，最后加载到界面上。

一开始无法读写是因为：根据路径访问时，系统变量$%SystemRoot%$无法自动翻译，无法访问路径。

只需要使用字符串Replace方法处理一下就可以了。

## 隐蔽性情况

这种绑定服务的自启动也是比较明显的。可能藏在大堆文件中，手工找出可能比较麻烦。

# 3.Drivers 系统驱动服务

## 技术原理及实现细节
### （1）注册表键

驱动程序比Logon简洁很多，所有配置文件位于下面的注册表键下：
```
HKLM\System\CurrentControlSet\Services
```
在这个键下面，还有很多子健，每一个子健都有自己的内部属性值。

下面给出对应的截图：
![image](https://user-images.githubusercontent.com/46391254/120929355-54761000-c71b-11eb-974a-4c9cb134922f.png)

可以看到，这里有许多键值，代表了不同信息。

### （2）start位标记自启动
可以根据注册表中start取值，得到不同的服务类型

倘若发现start=2，这就意味着这个是自启动的程序喽。

### （3）Drivers源文件路径分析

对所有内容进行分析，我们得到一些信息如下：

```
\SystemRoot\System32\drivers\1394ohci.sys
System32\Drivers\360AntiHacker64.sys
System32\Drivers\360AntiHijack64.sys
system32\DRIVERS\360FsFlt.sys
\??\C:\WINDOWS\system32\drivers\360reskit64.sys
\??\C:\WINDOWS\system32\drivers\360Sensor64.sys
System32\drivers\3ware.sys
System32\drivers\ACPI.sys
\SystemRoot\System32\drivers\AcpiDev.sys
\SystemRoot\System32\drivers\acpitime.sys
System32\drivers\ADP80XX.SYS
\SystemRoot\system32\drivers\afd.sys
```

对于所有的子健，内部元素有以下特征：

ImagePath路径需要给出一个统一的处理逻辑：

使用ImagePath+start筛选出有效项目，之后对路径进行筛选，匹配出含有.sys、.SYS的路径，最后再对路径进行格式化处理：

下面给出具体的处理逻辑：

\??\ 需要直接去掉，后面一般跟的都是绝对路径

SystemRoot需要修改为 %SystemRoot%

遇到System32，仍然是加入%SystemRoot%\\变量即可

### （4）获取对应路径的详细信息

所有的sys文件都被保护，无法取得对应的文件详细信息。其实是无法写入，但是只要路径正确，总归是可以读取的。

对于保护，提供了windows api，_PnPEntity类和Win32_PnPSignedDriver 类，用于驱动的写法处理。

无法写入的原因和上面的Services类似，路径无法自动翻译。

## 隐蔽性情况

这种绑定驱动的自启动也是比较明显的。可能藏在大堆文件中，手工找出可能比较麻烦。

查看驱动的自启动项，可以定为以下步骤：

读取注册表-》获得驱动程序sys目录-》获取详细信息

# 5.Scheduled Tasks 计划任务

## 技术原理及实现细节

Scheduled Tasks是windows的计划任务，当达到某个条件以后，就会自动运行。这也是可能存在自动运行的部分。

下面我们来分析一下各个API的实用性：

查阅到TaskSchedulerClass类，可以和计划任务管理程序连接，最后得到对应的计划任务，下面是简单的代码demo：

```
TaskSchedulerClass ts = new TaskSchedulerClass();
ts.Connect(null, null, null, null);
ITaskFolder folder = ts.GetFolder("\\");
IRegisteredTaskCollection task_exists = folder.GetTasks(1);
IRegisteredTask t = task_exists[0];
string description = t.Definition.Data;
string name = t.Name;
string ImagePath = t.Path;
```

我们将以上信息写入前台，观察效果：
![image](https://user-images.githubusercontent.com/46391254/120929377-65bf1c80-c71b-11eb-8144-cbd2c93520bc.png)

发现可以获取，但是没有获取完整，只有用户态的计划任务被找了出来，不满足我们的需求。

于是使用读取注册表的方法获取Scheduled Tasks。

两个主要的键是：
```
HKLM\Software\Microsoft\Windows NT\CurrentVersion\Schedule\Taskcache\Tasks
HKLM\Software\Microsoft\Windows NT\CurrentVersion\Schedule\Taskcache\Tree
```

首先分析Tasks里面的信息：

主要有以下几种情况：

1.Source、Author、Description三个字段，倘若以@开头，那就一定含有计划任务的绝对路径。

![image](https://user-images.githubusercontent.com/46391254/120929388-7374a200-c71b-11eb-80db-a0e05f56449d.png)


对这种路径格式解析，之后再处理目标文件即可。systemRoot是固定的目录。

![image](https://user-images.githubusercontent.com/46391254/120929392-77082900-c71b-11eb-81e9-04d41fc7c9bf.png)


这是其中的某一个反例，Description里面居然是中文字符串。

$(@%SystemRoot%\system32\dsregcmd.exe,-102)

据观察，只要有路径，一定是用@开头的。

另一种情况：

倘若以上三个字段都没有值，需要解析Actions字段

![image](https://user-images.githubusercontent.com/46391254/120929408-82f3eb00-c71b-11eb-8635-23439be9a807.png)

![image](https://user-images.githubusercontent.com/46391254/120929419-8ab38f80-c71b-11eb-94e8-228771585544.png)



可以明显看到，路径名存储在actions里面。

这个是解码后的字符串：

LocalSystemff<%windir%\system32\ProvTool.exe:/turn 5 /source LogonIdleTask

可以看到，是存在一些路径的，直接使用正则匹配表达式匹配即可。

也有可能这里也没有，那么就是找不到目标程序，需要标出异常，或者返回空。

最后再使用我们的处理框架即可获取这些自启动项的信息。

## 隐蔽性分析

在此处，某些信息会存储在注册表的一个二进制串中，很难发现。隐蔽性算是比较高的。


# 6.最后程序展示

使用了Shell32.dll文件，使用了这里面的API：提供文件路径，查看文件的详细信息。

程序主要是打开每种自启动项固定的注册表路径，解析里面键值的内容，最后根据得到的具体路径，用shell32 API获取详细信息，最后存储到结构体链表中。

在程序的最高层，循环链表，将内容填充到DataGridView控件里面。

使用C# 和 .Net框架，编写winform程序，实现了一个基础的页面。

![image](https://user-images.githubusercontent.com/46391254/120929427-91420700-c71b-11eb-85c9-851b9512e54a.png)

![image](https://user-images.githubusercontent.com/46391254/120929432-943cf780-c71b-11eb-8713-376e424646ec.png)

![image](https://user-images.githubusercontent.com/46391254/120929437-969f5180-c71b-11eb-857a-41d0ef93ea29.png)

![image](https://user-images.githubusercontent.com/46391254/120929439-9a32d880-c71b-11eb-9c28-5e5e6b86f1d0.png)

![image](https://user-images.githubusercontent.com/46391254/120929442-9dc65f80-c71b-11eb-969e-c9c7f7675cef.png)





可以看到，基本上实现了对四个基本自启动项的查看。
