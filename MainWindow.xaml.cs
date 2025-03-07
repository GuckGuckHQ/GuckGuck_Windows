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
// ...existing code...

namespace GuckGuck
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            // Assuming CaptureBorder is a named element in your XAML
            var captureBorder = CaptureBorder;
			var topLeft = captureBorder.PointToScreen(new Point(0, 0));
			var size = new Size(captureBorder.ActualWidth, captureBorder.ActualHeight);
			var inputRect = new System.Drawing.Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.Width, (int)size.Height);

			CaptureScreenshot(inputRect, "screenshot");
        }

        private void FullscreenCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            var inputRect = new System.Drawing.Rectangle(0, 0, screenWidth, screenHeight);

            CaptureScreenshot(inputRect, "fullscreen_screenshot");
        }

        private void CaptureScreenshot(System.Drawing.Rectangle rect, string fileNamePrefix)
        {
            var screenshotService = new ScreenshotService();
            var bytes = screenshotService.Capture(rect);
            Debug.WriteLine("got image: " + bytes.Length);

            // Save the screenshot to a file on the desktop
            using (var fs = System.IO.File.OpenWrite(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), fileNamePrefix + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png")))
            {
                fs.Write(bytes, 0, bytes.Length);
            }

            screenshotService.Dispose();
        }
    }
}