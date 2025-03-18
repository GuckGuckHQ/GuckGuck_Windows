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
        _screenshotTimerService = new ScreenshotTimerService(5 * 60 * 1000);
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
        StartButtonTextBlock.Text = "Start";
        OnAirTextBlock.Visibility = Visibility.Hidden;
    }
    else
    {
		StartButtonTextBlock.Text = "Stop";
        OnAirTextBlock.Visibility = Visibility.Visible;
        UpdateInputRect();
        _screenshotTimerService.Start();
        
        var guidPart = Guid.NewGuid().ToString("N");
        var randomPart = System.IO.Path.GetRandomFileName().Replace(".", "");
        var timestampPart = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        _currentId = $"{guidPart}{randomPart}{timestampPart}";
        
        await _screenshotTimerService.CaptureAndUploadScreenshot(_currentId);
    }
    _isCapturing = !_isCapturing;
    
    UrlTextBox.Text = $"{Constants.BaseUrl}/{_currentId}";
}


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
        if (int.TryParse(IntervalRun.Text, out int interval))
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
        IntervalRun.Text = interval.ToString();
        _screenshotTimerService.UpdateInterval(ConvertToMilliseconds(interval));
    }

    private void DecreaseInterval_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(IntervalRun.Text, out int interval) && interval > 0)
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
        IntervalRun.Text = interval.ToString();
        _screenshotTimerService.UpdateInterval(ConvertToMilliseconds(interval));
    }

    private int ConvertToMilliseconds(int interval)
    {
        switch (UnitButtonTextBlock.Text)
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
        switch (UnitButtonTextBlock.Text)
        {
            case "Seconds":
                UnitButtonTextBlock.Text = "Minutes";
                break;
            case "Minutes":
                UnitButtonTextBlock.Text = "Hours";
                break;
            case "Hours":
                UnitButtonTextBlock.Text = "Seconds";
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