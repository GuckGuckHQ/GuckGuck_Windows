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
	private readonly HttpClient _client;
	private string _currentId;
	public Rectangle InputRect { get; set; }

	public ScreenshotTimerService(double interval)
	{
		_timer = new Timer(interval);
		_timer.Elapsed += OnTimedEvent;
		_client = new HttpClient();
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

	public void UpdateInterval(double interval)
	{
		_timer.Interval = interval;
	}

	public void UpdateCurrentId(string id)
	{
		_currentId = id;
	}

	private async void OnTimedEvent(object sender, ElapsedEventArgs e)
	{
		await CaptureAndUploadScreenshot(_currentId);
	}

	public async Task CaptureAndUploadScreenshot(string id)
	{
		Debug.WriteLine("Capture: " + DateTime.Now.ToString());
		UpdateCurrentId(id);
		var screenshotService = new Screenshotter();
		var bytes = screenshotService.Capture(InputRect);

		//save screenshot to file
		//using (var fs = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), id + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png")))
		//{
		//	fs.Write(bytes, 0, bytes.Length);
		//}

		var content = new MultipartFormDataContent();
		content.Add(new ByteArrayContent(bytes), "Image", "screenshot.png");
		content.Add(new StringContent(_currentId), "Id");
		var response = await _client.PostAsync($"{Constants.BaseUrl}/image", content);
		screenshotService.Dispose();
	}

	public void Dispose()
	{
		_timer.Dispose();
		_client.Dispose();
	}
}