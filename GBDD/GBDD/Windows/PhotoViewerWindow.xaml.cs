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
using System.Windows.Shapes;

namespace GBDD.Windows
{
    /// <summary>
    /// Логика взаимодействия для PhotoViewerWindow.xaml
    /// </summary>
    public partial class PhotoViewerWindow : Window
    {
        private double _zoom = 1.0;
        public PhotoViewerWindow(string imagePath)
        {
            InitializeComponent();
            LoadImage(imagePath);
            SetupZoom();
        }
        private void LoadImage(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                imgPhoto.Source = bitmap;
                Title = $"Просмотр фотографии - {System.IO.Path.GetFileName(path)}";
            }
            catch
            {
                MessageBox.Show("Не удалось загрузить изображение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void SetupZoom()
        {
            imgPhoto.MouseWheel += ImgPhoto_MouseWheel;
        }

        private void ImgPhoto_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                _zoom *= 1.1;
            else
                _zoom /= 1.1;

            _zoom = Math.Max(0.1, Math.Min(_zoom, 10.0));

            ScaleTransform scaleTransform = new ScaleTransform(_zoom, _zoom);
            imgPhoto.RenderTransform = scaleTransform;
            imgPhoto.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }
}

