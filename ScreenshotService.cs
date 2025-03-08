using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SkiaSharp;
using System.Drawing;
using System.Windows;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.DXGI.Resource;

namespace GuckGuck;

public class ScreenshotService : IDisposable
{
	private Factory1 factory;
	private Adapter1 adapter;
	private Device device;
	private Output output;
	private Output1 output1;
	private Texture2DDescription textureDesc;
	private Texture2D screenTexture;
	private OutputDuplication duplicatedOutput;

	public ScreenshotService()
	{
		factory = new Factory1();
		adapter = factory.GetAdapter1(0);
		device = new Device(adapter);
		output = adapter.GetOutput(0);
		output1 = output.QueryInterface<Output1>();

		textureDesc = new Texture2DDescription
		{
			CpuAccessFlags = CpuAccessFlags.Read,
			BindFlags = BindFlags.None,
			Format = Format.B8G8R8A8_UNorm,
			Width = output.Description.DesktopBounds.Right,
			Height = output.Description.DesktopBounds.Bottom,
			OptionFlags = ResourceOptionFlags.None,
			MipLevels = 1,
			ArraySize = 1,
			SampleDescription = { Count = 1, Quality = 0 },
			Usage = ResourceUsage.Staging
		};

		screenTexture = new Texture2D(device, textureDesc);
		duplicatedOutput = output1.DuplicateOutput(device);
	}

	public byte[] Capture(Rectangle captureRegion)
	{
		// Get the current display scaling factor using PresentationSource
		PresentationSource source = null;
		DataBox mapSource = default;
        Rectangle scaledCaptureRegion = default;
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
		{
			var mainWindow = System.Windows.Application.Current.MainWindow;
			PresentationSource source = PresentationSource.FromVisual(mainWindow);

			if (source?.CompositionTarget == null)
			{
				throw new InvalidOperationException("Unable to determine the scaling factor.");
			}

			var transformToDevice = source.CompositionTarget.TransformToDevice;
			float scalingFactorX = (float)transformToDevice.M11;
			float scalingFactorY = (float)transformToDevice.M22;


			scaledCaptureRegion = new Rectangle(
				(int)(captureRegion.Left),
				(int)(captureRegion.Top),
				(int)(captureRegion.Width * scalingFactorX),
				(int)(captureRegion.Height * scalingFactorY)
			);

			Resource screenResource;
			OutputDuplicateFrameInformation duplicateFrameInformation;

			duplicatedOutput.TryAcquireNextFrame(500, out duplicateFrameInformation, out screenResource);
			using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
			{
				device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
			}
			screenResource.Dispose();
			duplicatedOutput.ReleaseFrame();

			mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);
		});
		using (var bitmap = new SKBitmap(textureDesc.Width, textureDesc.Height, SKColorType.Bgra8888, SKAlphaType.Premul))
		{
			var sourcePtr = mapSource.DataPointer;
			var destPtr = bitmap.GetPixels();
			for (int y = 0; y < textureDesc.Height; y++)
			{
				Utilities.CopyMemory(destPtr, sourcePtr, textureDesc.Width * 4);
				sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
				destPtr = IntPtr.Add(destPtr, bitmap.RowBytes);
			}
			device.ImmediateContext.UnmapSubresource(screenTexture, 0);

			using (var croppedBitmap = new SKBitmap(scaledCaptureRegion.Width, scaledCaptureRegion.Height))
			{
				using (var canvas = new SKCanvas(croppedBitmap))
				{
					canvas.DrawBitmap(bitmap, new SKRect(scaledCaptureRegion.Left, scaledCaptureRegion.Top, scaledCaptureRegion.Right, scaledCaptureRegion.Bottom), new SKRect(0, 0, scaledCaptureRegion.Width, scaledCaptureRegion.Height));
				}

				using (var image = SKImage.FromBitmap(croppedBitmap))
				using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
				{
					return data.ToArray();
				}
			}
		}
	}


	public void Dispose()
	{
		duplicatedOutput.Dispose();
		screenTexture.Dispose();
		device.Dispose();
		adapter.Dispose();
		factory.Dispose();
	}
}