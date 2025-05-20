using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LineMultiplicationApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnVisualizeClicked(object sender, EventArgs e)
    {
        if (int.TryParse(Number1Entry.Text, out int num1) && int.TryParse(Number2Entry.Text, out int num2))
        {
            var drawable = new LineMultiplicationDrawable(num1, num2, result =>
            {
                // 계산 결과 콜백 -> UI 업데이트
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResultLabel.Text = result;
                });
            });

            LineCanvas.Drawable = drawable;
            LineCanvas.Invalidate();
        }
    }

}