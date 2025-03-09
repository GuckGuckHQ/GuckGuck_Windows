using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuckGuck;

public partial class MainWindow : Window
{
    private ScreenshotTimerService _screenshotTimerService;

    public MainWindow()
    {
        InitializeComponent();
        _screenshotTimerService = new ScreenshotTimerService(5000);
        this.SizeChanged += MainWindow_SizeChanged;
        this.LocationChanged += MainWindow_LocationChanged;
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateInputRect();
    }

    private void MainWindow_LocationChanged(object sender, EventArgs e)
    {
        UpdateInputRect();
    }

    private void UpdateInputRect()
    {
        var captureBorder = CaptureBorder;
        var topLeft = captureBorder.PointToScreen(new Point(0, 0));
        var size = new Size(captureBorder.ActualWidth, captureBorder.ActualHeight);
        var inputRect = new System.Drawing.Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.Width, (int)size.Height);

        _screenshotTimerService.InputRect = inputRect;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private async void ScreenshotButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateInputRect();
        _screenshotTimerService.Start();
        await _screenshotTimerService.CaptureAndUploadScreenshot("screenshot");
    }

    private async void FullscreenCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        _screenshotTimerService.Start();
        var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
        var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
        var inputRect = new System.Drawing.Rectangle(0, 0, screenWidth, screenHeight);

        _screenshotTimerService.InputRect = inputRect;
        await _screenshotTimerService.CaptureAndUploadScreenshot("fullscreen_screenshot");
    }

    private void IncreaseInterval_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(IntervalTextBox.Text, out int interval))
        {
            interval++;
            IntervalTextBox.Text = interval.ToString();
        }
    }

    private void DecreaseInterval_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(IntervalTextBox.Text, out int interval) && interval > 0)
        {
            interval--;
            IntervalTextBox.Text = interval.ToString();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void UnitButton_Click(object sender, RoutedEventArgs e)
    {
        switch (UnitButton.Content.ToString())
        {
            case "Seconds":
                UnitButton.Content = "Minutes";
                break;
            case "Minutes":
                UnitButton.Content = "Hours";
                break;
            case "Hours":
                UnitButton.Content = "Seconds";
                break;
        }
    }
}