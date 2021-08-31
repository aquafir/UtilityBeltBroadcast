using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityBeltBroadcast
{
	static class Logger
	{
		internal static readonly bool Debugging;

		public static void Error(string msg)
		{
			Console.WriteLine("Error: " + msg);
		}
		public static void LogException(Exception ex)
		{
			LogException(ex.Message);
		}
		public static void LogException(string ex)
		{
			Console.WriteLine("Exception: " + ex);
		}
		public static void WriteToChat(string msg)
		{
			Console.WriteLine("Chat: " + msg);
		}

		public static void Debug(string msg)
		{
			Console.WriteLine("Debugging: " + msg);
		}
	}
}
