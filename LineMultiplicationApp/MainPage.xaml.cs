using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Numerics;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;


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

            float spacing = 120;
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

    double translateX = 0, translateY = 0;
    float scale = 1.0f;

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        scale = MathF.Min(3.0f, scale + 0.1f);
        LineCanvas.Scale = scale;
        LineCanvas.Invalidate();
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        scale = MathF.Max(0.5f, scale - 0.1f);
        LineCanvas.Scale = scale;
        LineCanvas.Invalidate();
    }

    double panX = 0, panY = 0;

    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panX = translateX;
                panY = translateY;
                break;

            case GestureStatus.Running:
                translateX = panX + e.TotalX;
                translateY = panY + e.TotalY;
                Redraw();
                break;
        }

    }
    void Redraw()
    {
        LineCanvas.Scale = scale;
        LineCanvas.TranslationX = translateX;
        LineCanvas.TranslationY = translateY;
        LineCanvas.Invalidate();
    }
}