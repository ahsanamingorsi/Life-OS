namespace LifeOS.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "Life OS", MinimumWidth = 360, MinimumHeight = 480 };
	}
}
