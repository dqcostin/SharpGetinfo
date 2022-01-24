using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;
using Shell32;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Sockets;
using System.Net;



namespace GetInfo
{
    class Program
    {
        public static void SystemInfo()
        {
            //获取系统信息
            Console.WriteLine("==========基本信息==========\n");
            var operating_system = Environment.OSVersion;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            Console.WriteLine("[+]机器名: " + Environment.MachineName);
            Console.WriteLine("[+]域名: " + Environment.UserDomainName);
            Console.WriteLine("[+]当前用户: " + Environment.UserName);
            Console.WriteLine("[+]NET版本: {0}", Environment.Version.ToString());
            Console.WriteLine("[+]操作系统：" + GetOSName()); //操作系统
            RunCMDCommand("[+]位数：", "wmic os get osarchitecture  | findstr \"32 || 64\"");
            internet.Internet();
            RunCMDCommand("[+]存在特权(可利用)：\n", "whoami /priv | findstr  \"SeImpersonatePrivilege SeAssignPrimaryPrivilege SeTcbPrivilege SeBackupPrivilege SeRestorePrivilege SeCreateTokenPrivilege SeLoadDriverPrivilege SeTakeOwnershipPrivilege SeDebugPrivilege\"");
            RunCMDCommand("[+]存在用户：\n", "net user |findstr \"%username%\"");
        }
        public static void Domain_p() //调用Domain进行域内信息探测
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            if (properties.DomainName.Length > 0)
            {
                Console.WriteLine("\n[+]该主机存在域！域名为:{0}", properties.DomainName);
                DoIt();
                domain.Domain();
                Console.WriteLine("\n");
                ZeroLogon.ZeroLogonCheck();
            }
            else
            {
                Console.WriteLine("\n[-]该主机不在域内，工作组信息收集完成~~");
            }
        }
        public static void DoIt()   //定位域控IP
        {
            DirectoryEntry dirEntry = new DirectoryEntry("LDAP://rootDSE");
            string dnsHostname = dirEntry.Properties["dnsHostname"].Value.ToString();
            Console.WriteLine("[+]域控FQDN：" + dnsHostname);
            IPAddress[] ipAddresses = Dns.GetHostAddresses(dnsHostname);
            Console.WriteLine("\n[+]域控IP为：");
            foreach (IPAddress i in ipAddresses)
            {
                Console.WriteLine(i);
            }
        }
        public static void RunCMDCommand(string commont, string command)
        {
            //运行系统命令函数
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "cmd.exe";
            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            CmdProcess.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            CmdProcess.StartInfo.Arguments = "/c " + command; //“/C”表示执行完命令后马上退出 
            CmdProcess.Start();//执行  


            Console.WriteLine(commont + CmdProcess.StandardOutput.ReadToEnd());

            CmdProcess.WaitForExit();//等待程序执行完退出进程  

            CmdProcess.Close();//结束  
        }//命令执行函数

        public static void readregedit()
        {
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey SYS = rk.OpenSubKey("system").OpenSubKey("CurrentControlSet").OpenSubKey("Control").OpenSubKey("Terminal Server");
            Console.WriteLine("[+] RDP信息：");
            foreach (string b in SYS.GetValueNames())//这里用shell.getvaluenames()不是shell.getsubkeynames() 
            {
                string a = SYS.GetValue(b).ToString();
                if (b == "fDenyTSConnections")
                {
                    string e = SYS.GetValue(b).ToString();
                    int num = int.Parse(e);
                    if (num == 1)
                    {
                        Console.WriteLine("\t[-]RDP未开启");
                    }
                    else
                    {
                        Console.WriteLine("\t[+]RDP开启");
                    }
                }

            }
        }
        public static void Recent_files()
        {
            //最近使用的文件
            string recents = @"Microsoft\Windows\Recent";
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string recentsPath = Path.Combine(userPath, recents);
            DirectoryInfo di = new DirectoryInfo(recentsPath);
            Console.WriteLine("\n[+] 最近使用文件：" + recentsPath);
            foreach (var file in di.GetFiles())
            {
                Console.WriteLine("\t" + file.Name);
            }
        }
        public static void Network_Connentions()
        {
            //NETWORK CONNECTIONS  网络连接
            Console.WriteLine("\n[+] 网络连接状态：");
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endPoints = ipProperties.GetActiveTcpListeners();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation info in tcpConnections)
            {
                String str = info.LocalEndPoint.Address.ToString();
                if (str.StartsWith("127.0.0.1"))
                {
                    continue;
                }
                Console.WriteLine("\tLocal : " + info.LocalEndPoint.Address.ToString() + ":" + info.LocalEndPoint.Port.ToString() + " - Remote : " + info.RemoteEndPoint.Address.ToString() + ":" + info.RemoteEndPoint.Port.ToString());
            }
        }
        public static void AvProcessEDRproduct()
        {
            //杀软检测
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "cmd.exe";
            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            CmdProcess.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            CmdProcess.StartInfo.Arguments = "/c " + "wmic /node:localhost /namespace:\\\\root\\SecurityCenter2 path AntiVirusProduct Get DisplayName | findstr /V /B /C:displayName || echo No Antivirus installed";//“/C”表示执行完命令后马上退出  
            CmdProcess.Start();//执行  


            Console.WriteLine("==========杀软信息==========\n\n" + CmdProcess.StandardOutput.ReadToEnd());

            CmdProcess.WaitForExit();//等待程序执行完退出进程  

            CmdProcess.Close();//结束  

        }
        static string GetOSName()
        {
            //得到主机名称
            return new ComputerInfo().OSFullName;
        }
        public static Shell32.Folder GetShell32Folder(object folder, Object shell, Type shellAppType)
        {
            return (Shell32.Folder)shellAppType.InvokeMember("NameSpace",
            System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { folder });
        }
        public static void GetRecycleBinFilenames()
        {
            //得到回收站的信息
            Console.WriteLine("\n[+] 回收站信息：");
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
            Object shell = Activator.CreateInstance(shellAppType);
            Folder recycleBin = GetShell32Folder(10, shell, shellAppType);

            foreach (FolderItem2 recfile in recycleBin.Items())
            {
                Console.WriteLine("\t" + recfile.Name);
            }

            Marshal.FinalReleaseComObject(shell);
        }

        static void Main()
        {
            SystemInfo();
            AvProcessEDRproduct();
            Network_Connentions();
            ListRDPConnections.ListRDPOutConnections();
            ListRDPConnections.ListRDPInConnections();
            GetRecycleBinFilenames();
            readregedit();
            Domain_p();
            Console.WriteLine("End!!");
        }
    }
}
