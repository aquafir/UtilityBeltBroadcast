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
using ZetaIpc.Runtime.Server;
using ZetaIpc.Runtime.Client;

namespace UtilityBeltBroadcast
{
	class Program
	{
		private const int IpcPort = 51204;
		private static ExNetworking network;
		static void Main(string[] args)
		{
			bool firstRun;
			using var mutex = new Mutex(true, typeof(Program).Namespace, out firstRun);

			//If the broadcaster is already running, pass the args as a chat command
			if (!firstRun)
			{
				if (args.Length > 0)
				{
					var command = String.Join(" ", args);
					var c = new IpcClient();
					c.Initialize(IpcPort);
					var rep = c.Send(command);
					//Console.WriteLine("Received: " + rep);
				}
				return;
			}

			//IPC
			var s = new IpcServer();
			s.Start(IpcPort);
			Logger.Debug($"Started IPC server on port {s.Port}");

			s.ReceivedRequest += (sender, args) =>
			{
				Logger.Debug($"Broadcasting command: {args.Request}");
				network.DoBroadcast(network.Clients.Select(c => c.Value), args.Request, 0);
				//args.Response = "OK";
				args.Handled = true;
			};

			//Otherwise start up networking and connect to a running UB server
			Task.Factory.StartNew(() =>
			{
				network = new ExNetworking("Broadcaster", "BC", "Duskfall");
				network.Init();

				while (true)
				{
					network.NetworkLoop();
					Thread.Sleep(100);
					//network.DoBroadcast(network.Clients.Select(c => c.Value), "/mt jump", 0);
				}
			});

			Console.ReadLine();
		}
	}
}
