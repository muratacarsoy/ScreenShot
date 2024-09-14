using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace ScreenShotWPF
{
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        const int SRCCOPY = 0x00CC0020;

        const int CAPTUREBLT = 0x40000000;

        private ScreenShots screenShots;

        public MainWindow()
        {
            InitializeComponent();
            screenShots = new ScreenShots();
            // ListBox kontrolünün adı lbFiles olarak belirlenmişti
            // DataContext özelliğine Binding ile alınacak veri atanır
            lbFiles.DataContext = screenShots;
            screenShots.RenderFiles();
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
        }

        private Bitmap CaptureRegion(Rectangle region)
        {
            IntPtr desktophWnd;
            IntPtr desktopDc;
            IntPtr memoryDc;
            IntPtr bitmap;
            IntPtr oldBitmap;
            uint dpi;
            bool success;
            Bitmap result;
            desktophWnd = GetDesktopWindow();
            dpi = GetDpiForWindow(desktophWnd);
            desktopDc = GetWindowDC(desktophWnd);
            memoryDc = CreateCompatibleDC(desktopDc);
            bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            oldBitmap = SelectObject(memoryDc, bitmap);
            success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, region.Left, region.Top, SRCCOPY | CAPTUREBLT);
            try
            {
                if (!success) { throw new Win32Exception(); }
                result = System.Drawing.Image.FromHbitmap(bitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(desktophWnd, desktopDc);
            }
            return result;
        }

        private Bitmap CaptureDesktop()
        {
            Rectangle desktop;
            Screen[] screens; // Windows.Forms.Screen türünde aktif ekranlar
            desktop = Rectangle.Empty;
            screens = Screen.AllScreens; //Aktif ekranları dizi olarak alır
            for (int i = 0; i < screens.Length; i++)
            {
                Screen screen;
                screen = screens[i];
                // Rectangle.Union metodu ile alınan her bir ekranın dikdörtgeni birleştirilir.
                desktop = Rectangle.Union(desktop, screen.Bounds);
            }
            Bitmap result = CaptureRegion(desktop);
            SaveScreenShot(result);
            return result;
        }

        private void SaveScreenShot(Bitmap bitmap)
        {
            if (!System.IO.Directory.Exists("screenshots"))
            {
                System.IO.Directory.CreateDirectory("screenshots");
            }
            bitmap.Save(System.IO.Path.Combine("screenshots", DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss.FFF") + ".png"));
            // Her görüntü alındığında liste (lbFiles) yenilenecektir
            screenShots.RenderFiles();
        }

        private void TakeScreenShot_Clicked(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Thread.Sleep(50);
            CaptureDesktop();
            this.Show();
        }
    }
}