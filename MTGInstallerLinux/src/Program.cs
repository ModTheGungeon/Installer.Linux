using System;
using Gtk;

namespace MTGInstallerLinux {
	public class MainClass {
		public static void Main(string[] args) {
			Application.Init();
			MainWindow win = new MainWindow();
			win.Show();
			Application.Run();
		}
	}
}
