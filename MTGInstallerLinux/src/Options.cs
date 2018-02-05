using System;
using Gtk;

namespace MTGInstallerLinux {
	public class Options : VBox {
		public CheckButton UseHTTPButton;
		public CheckButton SkipVersionChecksButton;
		public CheckButton ForceBackupButton;
		public CheckButton LeavePatchDLLsButton;

		public bool UseHTTP;
		public bool SkipVersionChecks;
		public bool ForceBackup;
		public bool LeavePatchDLLs;

		public Options() {
			PackStart(new Label("Advanced Options"), expand: false, fill: true, padding: 0);
			PackStart(UseHTTPButton = new CheckButton("Use insecure HTTP"), expand: false, fill: true, padding: 0);
			PackStart(SkipVersionChecksButton = new CheckButton("Skip version checks"), expand: false, fill: true, padding: 0);
			PackStart(ForceBackupButton = new CheckButton("Force a backup of the current state to be made"), expand: false, fill: true, padding: 0);
			PackStart(LeavePatchDLLsButton = new CheckButton("Don't clean up MonoMod patch DLLs"), expand: false, fill: true, padding: 0);

			UseHTTPButton.Clicked += (sender, e) => { UseHTTP = ((CheckButton)sender).Active; };
			SkipVersionChecksButton.Clicked += (sender, e) => { SkipVersionChecks = ((CheckButton)sender).Active; };
			ForceBackupButton.Clicked += (sender, e) => { ForceBackup = ((CheckButton)sender).Active; };
			LeavePatchDLLsButton.Clicked += (sender, e) => { LeavePatchDLLs = ((CheckButton)sender).Active; };
		}
	}
}
