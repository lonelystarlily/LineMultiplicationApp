namespace LineMultiplicationApp.WinUI
{
    public partial class WinUIApplication : MauiWinUIApplication
    {
        public WinUIApplication()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
