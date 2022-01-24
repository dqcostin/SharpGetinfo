using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;

namespace GetInfo
{

    class internet
    {
        public static void Internet()
        {
            // 本地计算机属性
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            string hostName = computerProperties.HostName;
            string domainName = computerProperties.DomainName;
            Console.WriteLine("Interface host name : {0}", hostName);
            Console.WriteLine("Interface domain name : {0}", domainName);
            // 获取到当前计算机的网络接口数组
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            if (adapters == null || adapters.Length < 1)
            {
                Console.WriteLine("计算机没有网络接口被发现.");
                return;
            }
            else
            {
                int number = adapters.Length;
                Console.WriteLine("发现{0}个网络接口 ", number);
            }
            // 遍历网络接口数组
            foreach (NetworkInterface adapter in adapters)
            {
                if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    // 如果不支持 IPV4版本
                    continue;
                }
                // 网络接口对象
                IPInterfaceProperties properties = adapter.GetIPProperties();
                // 网络接口名称
                string name = adapter.Name;
                // 网络接口类型
                NetworkInterfaceType interfaceType = adapter.NetworkInterfaceType;
                // 获取到屋里地址
                Console.WriteLine("接口名称：{0}", name);
                Console.WriteLine("接口类型：{0}", interfaceType);

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                try
                {
                    // 所有的配置的IP地址集合
                    UnicastIPAddressInformationCollection uipAddrs = adapterProperties.UnicastAddresses;
                    IEnumerator uipAddrEnum = uipAddrs.GetEnumerator();
                    Console.Write("IP地址：");
                    while (uipAddrEnum.MoveNext())
                    {
                        UnicastIPAddressInformation uipAddr = (UnicastIPAddressInformation)uipAddrEnum.Current;
                        Console.Write(uipAddr.Address.ToString() + " ");
                    }
                    Console.WriteLine();


                    // 所有的DHCP获取的地址集合

                    // 所有的网关地址集合

                    // 所有的DNS地址集合
                    IPAddressCollection ndsAddrs = adapterProperties.DnsAddresses;
                    IEnumerator ndsAddrEnum = ndsAddrs.GetEnumerator();
                    Console.Write("DNS地址：");
                    while (ndsAddrEnum.MoveNext())
                    {
                        IPAddress dnsAddr = (IPAddress)ndsAddrEnum.Current;
                        Console.Write(dnsAddr.ToString() + " ");
                    }
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception : {0}", ex);
                }

                // 网络接口的物理地址


                Console.WriteLine("\n");
            }
        }
    }
}
