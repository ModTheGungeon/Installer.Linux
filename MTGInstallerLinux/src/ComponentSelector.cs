using System;
using System.Collections.Generic;
using Gtk;
using MTGInstaller;
using MTGInstaller.YAML;

namespace MTGInstallerLinux {
	public class ComponentSelector : VBox {
		public List<ComponentVersion> SelectedComponents = new List<ComponentVersion>();
		public Dictionary<string, List<RadioButton>> RadioButtonMap = new Dictionary<string, List<RadioButton>>();

		public event EventHandler SelectionChanged;

		public ComponentSelector() {
			var installer = MainWindow.Instance.Installer;
			PackStart(new Label("Available components") { Visible = true }, expand: false, fill: true, padding: 0);

			foreach (var component in installer.AvailableComponents) {
				var component_name = component.Value.Name;
				var checkbox = new CheckButton(component_name.Replace("_", "__"));
				checkbox.Data["component_name"] = component_name;
				checkbox.Clicked += _CheckboxClicked;
				RadioButton main_radio_button = null;
				checkbox.Show();

				PackStart(checkbox, expand: false, fill: true, padding: 0);

				foreach (var ver in component.Value.Versions) {
					var radio = new RadioButton(main_radio_button, ver.DisplayName);

					radio.Data["component_version"] = new ComponentVersion(component.Value, ver);
					radio.Clicked += _RadioboxClicked;

					List<RadioButton> buttons;
					if (!RadioButtonMap.TryGetValue(component_name, out buttons)) {
						RadioButtonMap[component_name] = buttons = new List<RadioButton>();
					}
					buttons.Add(radio);

					var hbox = new HBox();
					hbox.PackStart(new Label("     "), expand: false, fill: true, padding: 0);
					hbox.PackStart(radio, expand: false, fill: true, padding: 0);
					PackStart(hbox, expand: false, fill: true, padding: 0);
					hbox.HideAll();
					//radio.Image = new Image(MainWindow.Instance.IconTheme.LoadIcon("web", 16, IconLookupFlags.UseBuiltin));

					if (main_radio_button == null) {
						main_radio_button = radio;
					}
				}
			}

			Show();
		}

		private void _CheckboxClicked(object sender, EventArgs e) {
			var checkbox = (CheckButton)sender;

			SelectedComponents.Clear();
			foreach (var pair in RadioButtonMap) {
				foreach (var button in pair.Value) {
					if (pair.Key == (string)checkbox.Data["component_name"]) {
						if (checkbox.Active) button.Parent.ShowAll();
						else button.Parent.HideAll();
					}

					if (button.Parent.Visible && button.Visible && button.Active) {
						SelectedComponents.Add((ComponentVersion)button.Data["component_version"]);
					}
				}
			}

			SelectionChanged.Invoke(this, new EventArgs());
		}

		private void _RadioboxClicked(object sender, EventArgs e) {
			SelectedComponents.Clear();
			foreach (var pair in RadioButtonMap) {
				foreach (var button in pair.Value) {
					if (button.Parent.Visible && button.Visible && button.Active) {
						SelectedComponents.Add((ComponentVersion)button.Data["component_version"]);
					}
				}
			}

			SelectionChanged.Invoke(this, new EventArgs());
		}
	}
}
