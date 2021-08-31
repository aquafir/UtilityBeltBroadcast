using System;
using System.Threading;
using System.Threading.Tasks;
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
			args = new string[] { "/mt jump" };
			bool firstRun;
			using var mutex = new Mutex(true, typeof(Program).Namespace, out firstRun);

			//If the broadcaster is already running, pass the args as a chat command
			if (firstRun)
			{
				StartIPC();
				StartUBNeworking("Broadcaster", "BC", "Duskfall");
			}

			//Process commands if there are any
			ProcessCommandline(args);

			Console.ReadLine();
		}

		private static void ProcessCommandline(string[] args)
		{
			if (args.Length > 0)
			{
				int retries = 0;
				while(network == null || network.Clients.Count() == 0)
				{
					Thread.Sleep(250);
					if (retries++ > 20)
					{
						Logger.Error("Failed to send command.  Not connected to UBNetServer.");
						return;
					}
				}
				var command = String.Join(" ", args);
				var c = new IpcClient();
				c.Initialize(IpcPort);
				var rep = c.Send(command);
				//Console.WriteLine("Received: " + rep);
			}
		}

		private static void StartUBNeworking(string name, string character, string world)
		{
			Task.Factory.StartNew(() =>
			{
				network = new ExNetworking(name, character, world);
				network.Init();

				while (true)
				{
					network.NetworkLoop();
					Thread.Sleep(100);
				}
			});
		}

		private static void StartIPC()
		{
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
		}
	}
}
