using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text.RegularExpressions;

namespace ClipboardRegex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime lastProcessed;
        static TimeSpan hundredMS = new TimeSpan(0, 0, 0, 0, 100);

        public MainWindow()
        {
            InitializeComponent();
            lastProcessed = DateTime.Now;
            
        }

        // Clipboard code taken from http://www.fluxbytes.com/csharp/how-to-monitor-for-clipboard-changes-using-addclipboardformatlistener/
        /// <summary>
        /// Places the given window in the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AddClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Removes the given window from the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Sent when the contents of the clipboard have changed.
        /// </summary>
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        // Event-processing code adapted from http://stackoverflow.com/a/1926796/
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

            bool result = AddClipboardFormatListener(source.Handle);
            if (!result)
            {
                var err = Marshal.GetLastWin32Error();
                Console.WriteLine(err);
            }
            Console.WriteLine(result);

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                if ((DateTime.Now - lastProcessed) > hundredMS) // Hack to avoid feedback loop
                {
                    IDataObject iData = Clipboard.GetDataObject();      // Clipboard's data.

                    /* Depending on the clipboard's current data format we can process the data differently. 
                     * Feel free to add more checks if you want to process more formats. */
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        string text = (string)iData.GetData(DataFormats.Text);
                        Clipboard.SetText(runUserRegex(text));
                    }
                    lastProcessed = DateTime.Now;
                }               
               
            }

            return IntPtr.Zero;
        }

        private string runUserRegex(string input)
        {
            if (!MultLineOpt.IsChecked.HasValue || !DotMatchNL.IsChecked.HasValue)
            {
                return input; // Should never happen
            }
                string pattern = FindWhat.Text;
                string replacement = ReplaceWith.Text;
                RegexOptions opts = (bool)MultLineOpt.IsChecked ? RegexOptions.Multiline : RegexOptions.None;
                opts |= (bool)DotMatchNL.IsChecked ? RegexOptions.Singleline : RegexOptions.None;

                try
                {
                    return System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement, opts, new TimeSpan(0, 0, 1));
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Invalid Regular Expression");
                    return input;
                }
                
        }

        


    }


    


}
