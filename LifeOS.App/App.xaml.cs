namespace LifeOS.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new MainPage()) { Title = "Life OS", MinimumWidth = 360, MinimumHeight = 480 };
#if WINDOWS
		// Started by "Start with Windows": open minimized so only reminders are active.
		if (Environment.GetCommandLineArgs().Contains("--minimized"))
		{
			bool done = false;
			window.Activated += (_, _) =>
			{
				if (done) return;
				done = true;
				if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window native
					&& native.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
					presenter.Minimize();
			};
		}
#endif
		return window;
	}
}
