using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
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
    private string _currentId;
    private bool _isCapturing = false;


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
        if (_isCapturing)
        {
            _screenshotTimerService.Stop();
            StartButton.Content = "Start";
        }
        else
        {
            StartButton.Content = "Stop";
            UpdateInputRect();
            _screenshotTimerService.Start();
            _currentId = Guid.NewGuid().ToString("N");
            await _screenshotTimerService.CaptureAndUploadScreenshot(_currentId);
        }
        _isCapturing = !_isCapturing;


        UrlTextBox.Text = $"http://guckguck.runasp.net/{_currentId}";
    }

    //private async void FullscreenCaptureButton_Click(object sender, RoutedEventArgs e)
    //{
    //    _screenshotTimerService.Start();
    //    var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
    //    var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
    //    var inputRect = new System.Drawing.Rectangle(0, 0, screenWidth, screenHeight);

    //    _screenshotTimerService.InputRect = inputRect;
    //    await _screenshotTimerService.CaptureAndUploadScreenshot("fullscreen_screenshot");
    //}

    private void IntervalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsTextAllowed(e.Text);
    }

    private static bool IsTextAllowed(string text)
    {
        Regex regex = new Regex("[^0-9]+"); // Regex that matches disallowed text
        return !regex.IsMatch(text);
    }

    private void IncreaseInterval_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(IntervalTextBox.Text, out int interval))
        {
            interval++;
        }
        else
        {
            interval = 1;
        }
        if (interval == 0)
        {
            interval = 1;
        }
        IntervalTextBox.Text = interval.ToString();
        _screenshotTimerService.UpdateInterval(ConvertToMilliseconds(interval));
    }

    private void DecreaseInterval_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(IntervalTextBox.Text, out int interval) && interval > 0)
        {
            interval--;
        }
        else
        {
            interval = 1;
        }
        if (interval == 0)
        {
            interval = 1;
        }
        IntervalTextBox.Text = interval.ToString();
        _screenshotTimerService.UpdateInterval(ConvertToMilliseconds(interval));
    }

    private int ConvertToMilliseconds(int interval)
    {
        switch (UnitButton.Content.ToString())
        {
            case "Minutes":
                return interval * 60 * 1000;
            case "Hours":
                return interval * 60 * 60 * 1000;
            case "Seconds":
            default:
                return interval * 1000;
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

    private void VisitButton_Click(object sender, RoutedEventArgs e)
    {
        var url = UrlTextBox.Text;
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show("URL cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}