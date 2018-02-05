using System;
using Gtk;
using MTGInstaller;
using MTGInstaller.YAML;

namespace MTGInstallerLinux {
	public class ExeSelector : HBox {
		public Entry Entry;
		public Button Button;
		public Gdk.Pixbuf Icon;

		public new string Path {
			get {
				return Entry.Text == "" ? "" : Entry.Text;
			}
			set {
				Entry.Text = value ?? "";
				Entry.Position = Entry.Text.Length;
			}
		}
		
		public ExeSelector() {
			Icon = MainWindow.Instance.IconTheme.LoadIcon("folder", 16, IconLookupFlags.UseBuiltin);

			Entry = new Entry();
			Entry.IsEditable = false;
			PackStart(Entry, expand: true, fill: true, padding: 0);
			Button = new Button(new Image(Icon));
			PackEnd(Button, expand: false, fill: false, padding: 0);

			Path = Autodetector.ExePath ?? "";
			Button.Clicked += SelectExe;
		}

		public void SelectExe(object sender, EventArgs e) {
			var fchooser = new FileChooserDialog(
				"Select the Enter the Gungeon executable...",
				MainWindow.Instance,
				FileChooserAction.Open,
				"Cancel", ResponseType.Cancel,
				"Open", ResponseType.Accept
			);
			var filter = new FileFilter();
			filter.AddPattern(Autodetector.ExeName);
			fchooser.AddFilter(filter);
			fchooser.ShowAll();
			var result = (ResponseType)fchooser.Run();
			if (result == ResponseType.Accept) {
				Path = fchooser.Filename;
			}
			fchooser.Destroy();
		}
	}
}