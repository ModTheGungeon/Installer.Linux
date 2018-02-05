using System;
using Gtk;

namespace MTGInstallerLinux {
	public class Log : TextView {
		public Log() {
			Editable = false;
			CursorVisible = false;
			Buffer.Text = "";
			WrapMode = WrapMode.Word;
		}

		private void _Focus() {
			Buffer.PlaceCursor(Buffer.EndIter);
			ScrollToMark(Buffer.InsertMark, 0, false, 0, 0);
		}

		public void Write(object o) {
			Buffer.Text += o.ToString();
			_Focus();
		}

		public void WriteLine() {
			Buffer.Text += '\n';
			_Focus();
		}

		public void WriteLine(object o) {
			Buffer.Text += o.ToString();
			Buffer.Text += '\n';
			_Focus();
		}
	}
}
