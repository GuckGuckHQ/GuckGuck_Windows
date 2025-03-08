using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Timers;

using Timer = System.Timers.Timer;

namespace GuckGuck;

public class ScreenshotTimerService : IDisposable
{
	private readonly Timer _timer;
	public Rectangle InputRect { get; set; }

    public ScreenshotTimerService(double interval)
	{
		_timer = new Timer(interval);
		_timer.Elapsed += OnTimedEvent;
	}

	public void Start()
	{
		_timer.Stop();
		_timer.Start();
	}

	public void Stop()
	{
		_timer.Stop();
	}


	private async void OnTimedEvent(object sender, ElapsedEventArgs e)
	{
		await CaptureAndUploadScreenshot("timed_screenshot");
	}

	public async Task CaptureAndUploadScreenshot(string fileNamePrefix)
	{
		var screenshotService = new Screenshotter();
		var bytes = screenshotService.Capture(InputRect);

		using (var fs = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileNamePrefix + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png")))
		{
			fs.Write(bytes, 0, bytes.Length);
		}

		var client = new HttpClient();
		var content = new MultipartFormDataContent();
		content.Add(new ByteArrayContent(bytes), "Image", "screenshot.png");
		content.Add(new StringContent("screenshot"), "Id");
		var response = await client.PostAsync("http://localhost:5137/image", content);
        screenshotService.Dispose();
	}

	public void Dispose()
	{
		_timer.Dispose();
	}
}