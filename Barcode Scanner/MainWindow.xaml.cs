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

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
            System.Diagnostics.Debug.WriteLine("Clicked");
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
