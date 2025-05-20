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
        if (int.TryParse(Number1Entry.Text, out int num1) &&
            int.TryParse(Number2Entry.Text, out int num2))
        {
            int digits1 = num1.ToString().Length;
            int digits2 = num2.ToString().Length;
            int totalDigits = digits1 + digits2;

            float spacing = 60;
            int lineLengthFactor;
            if (totalDigits <= 3)
                lineLengthFactor = 7;
            else if (totalDigits <= 5)
                lineLengthFactor = 6;
            else
                lineLengthFactor = totalDigits + 1;

            float width = (Math.Max(digits1, digits2) + lineLengthFactor + 2) * spacing;
            float height = (digits2 + 6) * spacing;

            LineCanvas.WidthRequest = width;
            LineCanvas.HeightRequest = height;

            var drawable = new LineMultiplicationDrawable(num1, num2, result =>
            {
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