using System;
using VaultBreaker.Core;
using VaultBreaker.Helpers;
using System.Threading;
using PowerArgs;
using System.Linq;

namespace VaultBreaker
{
    public class MyArgs
    {
        [ArgDescription("For supported password managers (Bitwarden, 1Password, DashLane), attempts to parse the memory for the Master Password to unlock the vault.")]
        public bool GetMaster { get; set; }

        [ArgDescription("The password manager type that's being targeted.")]
        public string Manager { get; set; }

        [ArgDescription("Launches a clipboard listener, waiting for clipboard events from common password manager applications")]
        public bool Monitor { get; set; }

        [ArgDescription("Used with Proxy flag, forces the application to be bounced without waiting for a lock screen notification.")]
        public bool Force { get; set; }

        [ArgDescription("Print out Debug Messages.")]
        public bool Debug { get; set; }
        
        [ArgDescription("Allows enumeration via a memory dump read in through a file.")]
        public bool Local { get; set; }
       
        [ArgDescription("(In Development) Launches an in-memory proxy, and attempts to re-launch supported password managers (Bitwarden/1Password) and intecept traffic.")]
        public bool Proxy { get; set; }

        [ArgDescription("(In Development) Attempts to dump the password database from in memory.")]
        public bool Dump { get; set; }

        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows Help")]
        public bool Help { get; set; }
    }

	public class VaultBreaker
	{
        static void Main(string[] args)
		{
            var parsed = Args.Parse<MyArgs>(args);
            if (parsed.Debug)
            {
                Globals.DebugMode = true;
                Console.WriteLine("[DEBUG] Debug mode enabled. Outputting debug messages");
            }
			//var outFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
			//var cache = MemoryCache.Default.PhysicalMemoryLimit;
			//Command Line Options
			//Local Flag allowing the processing of a memory dump from the local machine.

			//Get Processes
			if (parsed.Monitor)
			{
                Console.WriteLine("[+] Starting Clipboard Monitor");
                Monitoring.Start();
			}

			if (parsed.Local)
			{
                Console.WriteLine("[!] Not Implemented (yet)");
			}

			if (parsed.Proxy)
			{
                string[] supportedManagers = { "bitwarden", "1password" };
                if (String.IsNullOrEmpty(parsed.Manager))
                {
                    Console.WriteLine("[!] Please specify a password manager with the -Manager flag!");
                    return;
                }
                if (!supportedManagers.Any(parsed.Manager.ToString().ToLower().Contains))
                {
                    Console.WriteLine("[!] Sorry, that password manager is not yet supported");
                    return;
                }
				new Thread(() =>
				{
					Thread.CurrentThread.IsBackground = true;
					DebugFunctions.writeDebug("Starting Proxy. Press any key to quit.", Globals.DebugMode);
					ProxyHelper.startProxy();
				}).Start();
				switch (parsed.Manager.ToLower()) {
					case "bitwarden":
						BitWarden.LaunchWithProxy(parsed.Force);
						break;
					case "1password":
						_1Password.LaunchWithProxy(parsed.Force);
						break;
				}
				Console.ReadKey();
				DebugFunctions.writeDebug("Stopping Proxy.", Globals.DebugMode);
				ProxyHelper.DoQuit();
				DebugFunctions.writeDebug("Proxy stopped, Press any key to exit.", Globals.DebugMode);
				Console.ReadKey();
                return;
			}
			else if(parsed.GetMaster)
			{
                switch (parsed.Manager.ToLower())
				{
					case "bitwarden":
						Console.WriteLine("[+] Checking for Bitwarden Executables.");
						BitWarden.dumpBitwardenMaster();
						Console.ReadKey();
						break;
					case "1password":
						Console.WriteLine("[+] Checking for 1Password Executables.");
						_1Password.dump1passwordMaster();
						break;
					case "keepass":
                        //Initiate KeePass stuff.
                        Console.WriteLine("[-] Not currently implemented for KeePass, but checked out HarmJ0y's KeeThief located at https://github.com/HarmJ0y/KeeThief");
						break;
					case "dashlane":
						Dashlane.dumpDashlaneMaster();
						break;
					default:
                        Console.WriteLine("[-] Password manager not currently supported!");
						break;
				}
                return;
			}
            else if (parsed.Dump)
            {
                if (String.IsNullOrEmpty(parsed.Manager))
                {
                    Console.WriteLine("[!] Please specify a password manager with the -Manager flag!");
                    return;
                }
                switch (parsed.Manager)
                {
                    case "bitwarden":
                        Console.WriteLine("[!] This functionality is not yet supported!");
                        break;
                    default:
                        Console.WriteLine("[!] This functionality is not yet supported!");
                        break;
                }
                return;
            }
            else
            {
                Console.WriteLine("[!] No Options Selected");
            }
		}

	}
}
