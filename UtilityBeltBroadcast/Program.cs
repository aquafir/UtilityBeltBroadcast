using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using UtilityBelt.Tools;
using UtilityBelt;
using UBNetworking;
using UBNetworking.Lib;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Text.RegularExpressions;
using System.Linq;

namespace UtilityBeltBroadcast
{
	class Program
	{
		static void Main(string[] args)
		{
            using (var mutex = new Mutex(false, "com.UBBroadcastRelay.Instance"))
            {
                bool isAnotherInstanceOpen = !mutex.WaitOne(TimeSpan.Zero);
                if (isAnotherInstanceOpen)
                {
                    //Parse args and send via pipe?
                    //Try named pipe to send command from args
                    return;
                }

                //Otherwise start up networking and connect to a running UB server
                ExNetworking network;
                Task.Factory.StartNew(() =>
                {
                    network = new ExNetworking("Broadcaster","BC","Duskfall");
                    network.Init();

                    while (true)    
                    {
                        network.NetworkLoop();
                        Thread.Sleep(100);
                        network.DoBroadcast(network.Clients.Select(c => c.Value), "/mt jump", 0);
                    }
                });

            }

            Console.ReadKey();
		}
    }
}
