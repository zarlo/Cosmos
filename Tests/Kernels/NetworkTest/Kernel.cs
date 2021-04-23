using Cosmos.HAL;
using Cosmos.System.Network;
using Cosmos.System.Network.ARP;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace NetworkTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
        }

        protected override void Run()
        {
            NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0"); //get network device by name
            IPConfig.Enable(nic, new Address(10, 33, 20, 54), new Address(255, 255, 255, 0), new Address(10, 33, 20, 254)); //enable IPv4 configuration

            Console.WriteLine("IP: " + NetworkConfig.CurrentConfig.Value.IPAddress.ToString());

            using (var xClient = new TcpClient(4242))
            {
                if (xClient.Connect(new Address(10, 33, 20, 17), 4242) == true)
                {
                    Global.mDebugger.Send("TCP Connected!");
                }
                else
                {
                    Global.mDebugger.Send("TCP Connection FAILED!");
                    Sys.Power.Shutdown();
                }

                //xClient.Send(Encoding.ASCII.GetBytes("Hello world!"));

                //Global.mDebugger.Send("TCP Packet sent!");

                xClient.Close();

                Global.mDebugger.Send("TCP Connection Closed.");
            }

            Console.ReadKey();
            Sys.Power.Shutdown();
        }
    }
}
