using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Dynamsoft;
using Dynamsoft.DBR;
using System.Text;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using System.Buffers.Text;
using Windows.Graphics.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Barcode_Scanner
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public BarcodeReader Reader { get; set; }
        private MediaCapture _capture;
        private MediaFrameReader _frameReader;
        private MediaSource _mediaSource;
        private bool decoding = false;

        public MainWindow()
        {
            this.InitializeComponent();
            this.InitBarcodeReader();
        }

        private async void PickImageButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".pdf");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null) {
                TextResult[] results = Reader.DecodeFile(file.Path, "");
                System.Diagnostics.Debug.WriteLine(results.Length);
                DecodingResultsTextBox.Text = BuildResultsString(results);
            }
        }

        private string BuildResultsString(TextResult[] results) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Found " + results.Length
                .ToString() + " result(s).");
            for (int i = 0; i < results.Length; i++)
            {
                TextResult result = results[i];
                sb.Append(i + 1);
                sb.Append(". ");
                sb.Append(result.BarcodeFormatString);
                sb.Append(": ");
                sb.Append(result.BarcodeText);
                sb.Append('\n');
            }
            return sb.ToString();
        }

        private async void LiveScanButton_Click(object sender, RoutedEventArgs e) {
            ToggleCameraPanel(true);
            await InitializeCaptureAsync();
        }

        private void ToggleCameraPanel(bool on) 
        {
            CameraPanel.Visibility = on ? Visibility.Visible: Visibility.Collapsed;
            DefaultPanel.Visibility = on ? Visibility.Collapsed : Visibility.Visible;
        }
        
        //https://stackoverflow.com/questions/76956862/how-to-scan-a-qr-code-in-winui-3-using-webcam
        private async Task InitializeCaptureAsync()
        {
            // get first capture device (change this if you want)
            var sourceGroup = (await MediaFrameSourceGroup.FindAllAsync())?.FirstOrDefault();
            if (sourceGroup == null)
                return; // not found!

            // init capture & initialize
            _capture = new MediaCapture();
            await _capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                SourceGroup = sourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu, // to ensure we get SoftwareBitmaps
            });

            // initialize source
            var source = _capture.FrameSources[sourceGroup.SourceInfos[0].Id];

            // create reader to get frames & pass reader to player to visualize the webcam
            _frameReader = await _capture.CreateFrameReaderAsync(source, MediaEncodingSubtypes.Bgra8);
            _frameReader.FrameArrived += OnFrameArrived;
            await _frameReader.StartAsync();

            _mediaSource = MediaSource.CreateFromMediaFrameSource(source);
            player.Source = _mediaSource;
        }

        private async void OnFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var bmp = sender.TryAcquireLatestFrame()?.VideoMediaFrame?.SoftwareBitmap;
            if (bmp == null)
                return;
            if (decoding == true) {
                return;
            }
            decoding = true;
            using (var stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetSoftwareBitmap(bmp);
                await encoder.FlushAsync();
                var bytes = new byte[stream.Size];
                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
                TextResult[] results = Reader.DecodeFileInMemory(bytes, "");
                System.Diagnostics.Debug.WriteLine(results.Length);
                if (results.Length > 0)
                {
                    DispatcherQueue.TryEnqueue(async () => { 
                        DecodingResultsTextBox.Text = BuildResultsString(results);
                        await TerminateCaptureAsync();
                        ToggleCameraPanel(false);
                    });
                    
                }
            }
            decoding = false;
        }

        private async Task TerminateCaptureAsync()
        {
            player.Source = null;

            _mediaSource?.Dispose();
            _mediaSource = null;

            if (_frameReader != null)
            {
                _frameReader.FrameArrived -= OnFrameArrived;
                await _frameReader.StopAsync();
                _frameReader?.Dispose();
                _frameReader = null;
            }

            _capture?.Dispose();
            _capture = null;
        }

        private void InitBarcodeReader() {
            string errorMsg;
            EnumErrorCode errorCode = BarcodeReader.InitLicense("t0073oQAAACQUYF/x9rrKvXlLXTqx4ZcSmAZxrQ0Wdup4wSrA9cTE833mG4PB2q6PADieZ2scQd7NEVJ9LbY7AipZSK5QnFXf0H8iWQ==", out errorMsg);
            if (errorCode != EnumErrorCode.DBR_SUCCESS)
            {
                // Add your code for license error processing;
                System.Diagnostics.Debug.WriteLine(errorMsg);
            }
            Reader = new BarcodeReader();
            System.Diagnostics.Debug.WriteLine(Reader.GetVersion());
        }
    }
}
