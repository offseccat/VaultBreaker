using Fiddler;
using System;
using System.Collections.Generic;
using System.Threading;

namespace VaultBreaker.Helpers
{
	public class ProxyHelper
	{
		public static void DoQuit()
		{
			DebugFunctions.writeDebug("Shutting down...");
			Fiddler.FiddlerApplication.Shutdown();
			Thread.Sleep(500);
		}

		public static void startProxy()
		{
			List<Fiddler.Session> oAllSessions = new List<Fiddler.Session>();
			Fiddler.FiddlerApplication.SetAppDisplayName("VaultBreaker");
			Fiddler.FiddlerApplication.AfterSessionComplete += AfterSessionComplete;
			Fiddler.FiddlerApplication.BeforeRequest += delegate (Fiddler.Session oS)
			{
				// Console.WriteLine("Before request for:\t" + oS.fullUrl);
				// In order to enable response tampering, buffering mode MUST
				// be enabled; this allows FiddlerCore to permit modification of
				// the response in the BeforeResponse handler rather than streaming
				// the response to the client as the response comes in.
				oS.bBufferResponse = false;
				Monitor.Enter(oAllSessions);
				oAllSessions.Add(oS);
				Monitor.Exit(oAllSessions);
			};

			DebugFunctions.writeDebug(String.Format("Starting {0}...", Fiddler.FiddlerApplication.GetVersionString()));
			Fiddler.CONFIG.IgnoreServerCertErrors = true;
			CONFIG.bMITM_HTTPS = true;

			FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);
			
			//What port you want to listen on.
			ushort iPort = 8888;

			FiddlerCoreStartupSettings startupSettings =
				new FiddlerCoreStartupSettingsBuilder()
					.ListenOnPort(iPort)
					.DecryptSSL()
					.MonitorAllConnections()
					.OptimizeThreadPool()
					.Build();

			Fiddler.FiddlerApplication.Startup(startupSettings);

			FiddlerApplication.Log.LogFormat("Created endpoint listening on port {0}", iPort);
		}

		static void AfterSessionComplete(Session sess)
		{
			if(sess.RequestMethod == "CONNECT")
			{
				return;
			}
			string body = sess.GetRequestBodyAsString();

			DebugFunctions.writeDebug(String.Format("Recieved POST with the following body\r\n[DEBUG] {0}\r\n[DEBUG]{1}", body, sess.RequestMethod));

			/**
			if (sess.RequestMethod == "POST")
			{
				string body = sess.GetRequestBodyAsString();
				if(body != "")
				{
					DebugFunctions.writeDebug(String.Format("Recieved POST with the following body\r\n[DEBUG] {0}\r\n[DEBUG]{1}",body,sess.RequestMethod));
				}

			}
	**/
		}
	}
}
