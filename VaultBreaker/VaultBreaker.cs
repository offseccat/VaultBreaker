using System;
using VaultBreaker.Core;
using VaultBreaker.Helpers;
using System.Threading;

namespace VaultBreaker
{
	public class VaultBreaker
	{

		static void Main(string[] args)
		{
			var local = true;
			var monitor = false;
			var proxy = true;
			var force = true;
			var managerType = "1password";
			//var outFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
			//var cache = MemoryCache.Default.PhysicalMemoryLimit;
			//Command Line Options
			//Local Flag allowing the processing of a memory dump from the local machine.

			//Get Processes
			if (monitor)
			{

			}

			if (local)
			{

			}

			if (proxy)
			{
				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					DebugFunctions.writeDebug("Starting Proxy. Press any key to quit.");
					ProxyHelper.startProxy();
				}).Start();
				switch (managerType.ToLower()) {
					case "bitwarden":
						BitWarden.LaunchWithProxy(force);
						break;
					case "1password":
						_1Password.LaunchWithProxy(force);
						break;
				}
				Console.ReadKey();
				DebugFunctions.writeDebug("Stopping Proxy.");
				ProxyHelper.DoQuit();
				DebugFunctions.writeDebug("Proxy stopped, Press any key to exit.");
				Console.ReadKey();
				Environment.Exit(0);
				
			}
			else
			{
				switch (managerType.ToLower())
				{
					case "bitwarden":
						Console.WriteLine("Checking for Bitwarden Executables. Press Any key to continue.");
						Console.ReadKey();
						BitWarden.dumpBitwarden();
						Console.ReadKey();
						break;
					case "1password":
						Console.WriteLine("Checking for 1Password Executables. Press Any key to continue.");
						Console.ReadKey();
						_1Password.dump1password();
						break;
					case "keepass":
						//Initiate KeePass stuff.
						break;
					case "dashlane":
						Dashlane.dumpDashlane();
						break;
					default:
						break;
				}
			}
		}

	}
}
