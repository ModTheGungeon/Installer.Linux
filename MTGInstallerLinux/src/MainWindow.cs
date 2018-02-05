using System;
using System.Reflection;
using System.Threading.Tasks;
using Gtk;
using MTGInstaller;

namespace MTGInstallerLinux {
	public class MainWindow : Window {
		public static string Version {
			get {
				var attr = Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
				return attr?.InformationalVersion ?? "???";
			}
		}

		public static MainWindow Instance;

		public InstallerFrontend Installer;
		public IconTheme IconTheme = new IconTheme();

		public HBox MainBox;
		public VBox LeftPane;
		public VBox RightPane;

		public Log Log;

		public Image Robot;
		public ExeSelector ExeSelector;
		public Options Options;
		public ComponentSelector ComponentSelector;
		public HBox ButtonHBox;
		public Progress Progress;
		public Button InstallButton;
		public Button UninstallButton;

		private Logger.Subscriber _LoggerSubscriber;
		private bool _Robot = true;

		public MainWindow() : base(WindowType.Toplevel) {
			Instance = this;

			var robot_pixbuf = Gdk.Pixbuf.LoadFromResource("icon");
			Icon = robot_pixbuf;
			Title = $"Mod the Gungeon Installer {Version} (core {MTGInstaller.Installer.Version})";

			Installer = new InstallerFrontend();

			DefaultSize = new Gdk.Size(640, 480);
			DeleteEvent += OnDeleteEvent;

			MainBox = new HBox();
			LeftPane = new VBox();
			MainBox.Add(LeftPane);
			RightPane = new VBox();
			MainBox.Add(RightPane);
			MainBox.ShowAll();

			Add(MainBox);

			LeftPane.PackStart(new Label("Output"), expand: false, fill: false, padding: 0);
			LeftPane.PackStart(Robot = new Image(robot_pixbuf), expand: true, fill: true, padding: 0);
			LeftPane.PackStart(Log = new Log(), expand: true, fill: true, padding: 0);
			Log.SetSizeRequest(DefaultSize.Width / 2, DefaultSize.Height);
			Robot.SetSizeRequest(DefaultSize.Width / 2, DefaultSize.Height);
			Log.WriteLine("Installer ready!");
			LeftPane.ShowAll();
			Log.Hide();

			RightPane.PackStart(new Label("Choose the Enter the Gungeon executable"), expand: false, fill: true, padding: 0);			
			RightPane.PackStart(ExeSelector = new ExeSelector(), expand: false, fill: false, padding: 0);
			RightPane.PackStart(Progress = new Progress(), expand: true, fill: true, padding: 0);
			RightPane.ShowAll();
			RightPane.PackStart(ComponentSelector = new ComponentSelector(), expand: true, fill: true, padding: 0);
			ComponentSelector.SelectionChanged += (sender, e) => _UpdateInstallButton();
			RightPane.PackStart(Options = new Options(), expand: true, fill: true, padding: 0);
			Options.ShowAll();

			Progress.Hide();

			RightPane.PackStart(ButtonHBox = new HBox(), expand: false, fill: true, padding: 0);

			ButtonHBox.PackStart(InstallButton = new Button("Install"), expand: true, fill: true, padding: 0);
			InstallButton.Sensitive = false;
			InstallButton.Clicked += _Install;
			ButtonHBox.PackStart(UninstallButton = new Button("Uninstall"), expand: false, fill: false, padding: 0);
			UninstallButton.Clicked += _Uninstall;

			ButtonHBox.ShowAll();

			_LoggerSubscriber = new Logger.Subscriber((logger, loglevel, indent, str) => {
				_InvokeOnMainThread(() => { Log.WriteLine(logger.String(loglevel, str, indent)); });
			});
			Logger.Subscribe(_LoggerSubscriber);
		}

		protected void OnDeleteEvent(object sender, DeleteEventArgs e) {
			Application.Quit();
			e.RetVal = true;
		}

		private void _UpdateInstallButton() {
			if (ComponentSelector.SelectedComponents.Count == 0 || ExeSelector.Path == null) {
				InstallButton.Sensitive = false;
			} else InstallButton.Sensitive = true;
		}

		private void _Block() {
			ExeSelector.Sensitive = false;
			ComponentSelector.Sensitive = false;
			Options.Sensitive = false;
			InstallButton.Sensitive = false;
			UninstallButton.Sensitive = false;
		}

		private void _Unblock() {
			ExeSelector.Sensitive = true;
			ComponentSelector.Sensitive = true;
			Options.Sensitive = true;
			_UpdateInstallButton();
			UninstallButton.Sensitive = true;
		}

		private void _ShowLog() {
			if (_Robot) {
				Robot.Hide();
				Log.Show();
			}
		}

		private void _SwitchToInstalling() {
			_ShowLog();
			_Block();
			Progress.Show();
			Progress.Update(ComponentSelector.SelectedComponents);
			Progress.Start();
			Options.Hide();
			ComponentSelector.Hide();
		}

		private void _SwitchToUninstalling() {
			_ShowLog();
			_Block();
		}

		private void _SwitchToIdle() {
			_Unblock();
			Progress.Hide();
			Options.Show();
			ComponentSelector.Show();
		}

		private void _InvokeOnMainThread(System.Action action) {
			Application.Invoke((sender, e) => { action.Invoke(); });
		}

		private void _Install(object sender, EventArgs e) {
			_SwitchToInstalling();
			Log.WriteLine("Installing...");
			var task = Task.Run(() => {
				Exception ex = null;
				try {
					Installer.Options = InstallerFrontend.InstallerOptions.None;
					if (Options.UseHTTP) Installer.Options |= InstallerFrontend.InstallerOptions.HTTP;
					if (Options.ForceBackup) Installer.Options |= InstallerFrontend.InstallerOptions.ForceBackup;
					if (Options.SkipVersionChecks) Installer.Options |= InstallerFrontend.InstallerOptions.SkipVersionChecks;
					if (Options.LeavePatchDLLs) Installer.Options |= InstallerFrontend.InstallerOptions.LeavePatchDLLs;

					Installer.Install(ComponentSelector.SelectedComponents, ExeSelector.Path ?? Autodetector.ExePath, (component) => {
						Progress.Advance();
					});
				} catch (Exception install_ex) {
					ex = install_ex;
				}
				_InvokeOnMainThread(() => { _Post(ex, what: "installing"); });
			});
		}

		private void _Post(Exception exception, string what) {
			if (exception != null) {
				Log.WriteLine();
				Log.WriteLine($"Error while {what}:");
				Log.WriteLine(exception.ToString());

				var md = new MessageDialog(
					this,
					DialogFlags.DestroyWithParent | DialogFlags.Modal,
					MessageType.Error,
					ButtonsType.Close,
					$"Error while installing: {exception.Message}"
				);

				md.Run();
				md.Destroy();
			} else {
				Log.WriteLine();
				Log.WriteLine("Done.");
			}
			_SwitchToIdle();
		}

		private void _Uninstall(object sender, EventArgs e) {
			_SwitchToUninstalling();
			Log.WriteLine("Uninstalling...");
			var task = Task.Run(() => {
				Exception ex = null;
				try {
					Installer.Uninstall(ExeSelector.Path ?? Autodetector.ExePath);
				} catch (Exception uninstall_ex) {
					ex = uninstall_ex;
				}
				_InvokeOnMainThread(() => { _Post(ex, what: "uninstalling"); });
			});
		}
	}
}
