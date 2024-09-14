using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenShotWPF
{
    public class ScreenShots
    {
        public ScreenShots()
        {
            Files = new ObservableCollection<ScreenShot>();
        }

        public ObservableCollection<ScreenShot> Files { get; set; }

        public void RenderFiles()
        {
            Files.Clear();
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo("screenshots");
            if (!directory.Exists) { return; }
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(file.FullName, UriKind.Absolute);
                image.EndInit();
                ImageSource source = image;
                Files.Add(new ScreenShot() { Name = file.Name.Replace(".png", ""), Image = image });
            }
        }
    }

    public class ScreenShot
    {
        public ScreenShot() { }
        public string Name { get; set; }
        public BitmapImage Image { get; set; }
    }
}
