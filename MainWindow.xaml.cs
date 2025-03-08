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
        _screenshotTimerService.Start();
        var captureBorder = CaptureBorder;
        var topLeft = captureBorder.PointToScreen(new Point(0, 0));
        var size = new Size(captureBorder.ActualWidth, captureBorder.ActualHeight);
        var inputRect = new System.Drawing.Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.Width, (int)size.Height);

        _screenshotTimerService.UpdateInputRect(inputRect);
        await _screenshotTimerService.CaptureAndUploadScreenshot(inputRect, "screenshot");
    }

    private async void FullscreenCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        _screenshotTimerService.Start();
        var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
        var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
        var inputRect = new System.Drawing.Rectangle(0, 0, screenWidth, screenHeight);

        _screenshotTimerService.UpdateInputRect(inputRect);
        await _screenshotTimerService.CaptureAndUploadScreenshot(inputRect, "fullscreen_screenshot");
    }
}