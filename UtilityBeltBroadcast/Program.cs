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
		private static bool firstRun;
		private static readonly string EXIT_STRING = "exit-ubb";
		private static ExNetworking network;
		private static IpcServer ipcServer;
		static void Main(string[] args)
		{
			using var mutex = new Mutex(true, typeof(Program).Namespace, out firstRun);

			//Start UBNetworkServer and UBB's IPC server if needed
			if (firstRun)
			{
				Logger.Debug("Starting broadcaster");
				Task.Factory.StartNew(() =>
				{
					StartIPC();
					StartUBNeworking("Broadcaster", "BC", "Duskfall");
				});
				ProcessCommandline(args);

				//TODO: fix lazy way of keeping host process alive
				while (true)
					Thread.Yield();
			}
			ProcessCommandline(args);
		}

		private static void ProcessCommandline(string[] args)
		{
			//Stall for connection
			if (args.Length > 0 && firstRun)
			{
				int retries = 0;
				while (network == null || network.Clients.Count() == 0)
				{
					Thread.Sleep(1000);
					if (retries++ > 20)
					{
						Logger.Error("Failed to send command.  Not connected to UBNetServer.");
						return;
					}
				}
			}
			//Broadcast command
			var command = String.Join(" ", args);
			var c = new IpcClient();
			c.Initialize(IpcPort);
			var rep = c.Send(command);
		}

		private static void StartUBNeworking(string name, string character, string world)
		{
			network = new ExNetworking(name, character, world);
			network.Init();

			while (true)
				{
					network.NetworkLoop();
					Thread.Sleep(200);
				}
		}

		private static void StartIPC()
		{
			var s = new IpcServer();
			s.Start(IpcPort);
			Logger.Debug($"Started IPC server on port {s.Port}");

			s.ReceivedRequest += (sender, args) =>
			{
				if (string.Equals(args.Request, EXIT_STRING, StringComparison.InvariantCultureIgnoreCase))
				{
					Logger.Debug("Closing application.");
					Environment.Exit(0);
				}

				//Logger.Debug($"Broadcasting command: {args.Request}");
				network.DoBroadcast(network.Clients.Select(c => c.Value), args.Request, 0);
				//args.Response = "OK";
				args.Handled = true;
			};
		}
	}
}
