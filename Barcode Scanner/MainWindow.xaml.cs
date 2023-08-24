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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Barcode_Scanner
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public BarcodeReader reader { get; set; }
        public MainWindow()
        {
            this.InitializeComponent();
            this.InitBarcodeReader();
        }

        private async void pickImageButton_Click(object sender, RoutedEventArgs e)
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

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null) {
                TextResult[] results = reader.DecodeFile(file.Path, "");
                System.Diagnostics.Debug.WriteLine(results.Length);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Found "+results.Length
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
                decodingResultsTextBox.Text = sb.ToString();
            }
            System.Diagnostics.Debug.WriteLine(file.DisplayName);
            System.Diagnostics.Debug.WriteLine("Clicked");
        }

        private void liveScanButton_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Debug.WriteLine("live scan Clicked");
            CameraPanel.Visibility = Visibility.Visible;
            DefaultPanel.Visibility = Visibility.Collapsed;
        }

        private void InitBarcodeReader() {
            string errorMsg;
            EnumErrorCode errorCode = BarcodeReader.InitLicense("t0073oQAAACQUYF/x9rrKvXlLXTqx4ZcSmAZxrQ0Wdup4wSrA9cTE833mG4PB2q6PADieZ2scQd7NEVJ9LbY7AipZSK5QnFXf0H8iWQ==", out errorMsg);
            if (errorCode != EnumErrorCode.DBR_SUCCESS)
            {
                // Add your code for license error processing;
                System.Diagnostics.Debug.WriteLine(errorMsg);
            }
            reader = new BarcodeReader();
            System.Diagnostics.Debug.WriteLine(reader.GetVersion());
        }
    }
}
