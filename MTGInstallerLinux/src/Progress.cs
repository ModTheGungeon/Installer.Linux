using System;
using System.Collections.Generic;
using Gtk;
using MTGInstaller;

namespace MTGInstallerLinux {
	public class Progress : TreeView {
		public ListStore List;
		public TreeViewColumn NameColumn;
		public TreeViewColumn VersionColumn;
		private int _Position = 0;
		private bool _ForceSelect = false;

		public Progress() {
			Selection.SelectFunction = (selection, model, path, path_currently_selected) => {
				if (_ForceSelect) {
					_ForceSelect = false;
					return true;
				}
				return false;
			};

			HeadersVisible = false;
			NameColumn = new TreeViewColumn { Title = "Name" };
			AppendColumn(NameColumn);
			var cell = new CellRendererText();
			NameColumn.PackStart(cell, true);
			NameColumn.AddAttribute(cell, "text", 0);

			VersionColumn = new TreeViewColumn { Title = "Version" };
			AppendColumn(VersionColumn);
			var cell2 = new CellRendererText();
			VersionColumn.PackStart(cell2, true);
			VersionColumn.AddAttribute(cell2, "text", 1);

			Model = List = new ListStore(typeof(string), typeof(string));
		}

		public void Update(List<ComponentVersion> components) {
			List.Clear();
			foreach (var com in components) {
				List.AppendValues(com.Component.Name, com.Version.DisplayName);
			}
		}

		private void _UpdatePosition() {
			_ForceSelect = true;
			SetCursor(new TreePath(new int[] { _Position }), NameColumn, false);
		}

		public void Start() {
			_Position = 0;
			_UpdatePosition();
		}

		public void Advance() {
			_Position += 1;
			_UpdatePosition();
		}
	}
}