using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace VaultBreaker.Helpers
{
	class MemoryHelper
	{
		public static string dumpProcessMemory(Process proc)
		{
            DebugFunctions.writeDebug("Starting Memory Dump", true);
            //Dumps Process Memory, Converts it to string array and returns. No writing to file needed.
            DebugFunctions.writeDebug("Opening Process", true);
            IntPtr hProc = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.QueryInformation | WinAPI.ProcessAccessFlags.VirtualMemoryRead, false, proc.Id);
            DebugFunctions.writeDebug("OpenProcess Returned", true);
            WinAPI.MEMORY_BASIC_INFORMATION64 mbi = new WinAPI.MEMORY_BASIC_INFORMATION64();
            //32 bit
            //WinAPI.MEMORY_BASIC_INFORMATION mbi32 = new WinAPI.MEMORY_BASIC_INFORMATION();
			WinAPI.SYSTEM_INFO si = new WinAPI.SYSTEM_INFO();
			if (hProc == IntPtr.Zero)
			{
				//Failed.
				Console.WriteLine("Unable to create a connection to the process! Error Code: {0}", WinAPI.GetLastError());
				Environment.Exit(6);
			}
            DebugFunctions.writeDebug("Process Handle isn't 0", true);
            WinAPI.GetSystemInfo(out si);
			IntPtr hProc_min_addr = si.minimumApplicationAddress;
			IntPtr hProc_max_addr = si.maximumApplicationAddress;
			long hProc_long_min = (long)hProc_min_addr;
			long hProc_long_max = (long)hProc_max_addr;
			int bytesRead = 0;
            StringBuilder sb = new StringBuilder();
            while (hProc_long_min < hProc_long_max)
			{
                bytesRead = WinAPI.VirtualQueryEx(hProc, hProc_min_addr, out mbi, (uint)Marshal.SizeOf(typeof(WinAPI.MEMORY_BASIC_INFORMATION64)));
                //Console.WriteLine(mbi.AllocationBase + "\r\n" + mbi.AllocationProtect + "\r\n" + mbi.BaseAddress + "\r\n" + mbi.Protect + "\r\n" + mbi.RegionSize + "\r\n" + mbi.State + "\r\n" + mbi.Type + "\r\n" + mbi.__alignment1 + "\r\n" + mbi.__alignment2);
                //DebugFunctions.writeDebug(String.Format("Reading Memory - Current Location: {0}/{1}", hProc_long_min.ToString(), hProc_long_max.ToString()), Globals.DebugMode);
                if (mbi.Protect == WinAPI.PAGE_READWRITE && mbi.State == WinAPI.MEM_COMMIT)
				{
					byte[] buffer = new byte[mbi.RegionSize];
                    WinAPI.ReadProcessMemory(hProc, mbi.BaseAddress, buffer, mbi.RegionSize, ref bytesRead);
					for (long i = 0; i < mbi.RegionSize; i++)
					{
						sb.Append((char)buffer[i]);
					}
				}
                //hProc_long_min += (int)Marshal.SizeOf(typeof(WinAPI.MEMORY_BASIC_INFORMATION64));
                hProc_long_min += mbi.RegionSize;
				hProc_min_addr = new IntPtr(hProc_long_min);
			}
            //sw.Close();
            DebugFunctions.writeDebug("Finished Reading Memory, returning memory string.", Globals.DebugMode);
			return sb.ToString();
		}
	}
}

