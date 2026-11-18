using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MathMenu.controls
{
    /// <summary>
    /// Показ графика ScottPlot в обычном Image.
    /// Пакет ScottPlot.WPF в проект не добавлен, поэтому картинка
    /// получается через ScottPlot.Plot.GetImageBytes и BitmapImage.
    /// </summary>
    public class PlotCanvas
    {
        private readonly Image _image;
        private readonly DispatcherTimer _timer;
        private ScottPlot.Plot _plot;

        /// <summary>Последняя ошибка отрисовки (для отладки).</summary>
        public string LastError { get; private set; }

        public PlotCanvas(Image image)
        {
            _image = image;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(120);
            _timer.Tick += Timer_Tick;
        }

        public void Show(ScottPlot.Plot plot)
        {
            _plot = plot;
            Render();
        }

        public void Clear()
        {
            _plot = null;
            _image.Source = null;
        }

        /// <summary>Вызывается из SizeChanged области графика.</summary>
        public void Resize()
        {
            if (_plot == null)
            {
                return;
            }

            _timer.Stop();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            Render();
        }

        private void Render()
        {
            if (_plot == null)
            {
                return;
            }

            // у Image без Source собственная ActualWidth равна нулю,
            // поэтому размер берём у родительской рамки
            FrameworkElement host = _image.Parent as FrameworkElement;
            double w = _image.ActualWidth;
            double h = _image.ActualHeight;
            if (host != null && host.ActualWidth > 20 && host.ActualHeight > 20)
            {
                w = host.ActualWidth;
                h = host.ActualHeight;
                double border = 0;
                Border b = host as Border;
                if (b != null)
                {
                    border = b.BorderThickness.Left + b.BorderThickness.Right;
                    h -= b.BorderThickness.Top + b.BorderThickness.Bottom;
                    w -= border;
                }
            }
            if (w < 20 || h < 20)
            {
                return;
            }

            DpiScale dpi = VisualTreeHelper.GetDpi(_image);
            int px = (int)(w * dpi.DpiScaleX);
            int py = (int)(h * dpi.DpiScaleY);

            try
            {
                byte[] bytes = _plot.GetImageBytes(px, py, ScottPlot.ImageFormat.Png);

                BitmapImage bmp = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                }

                _image.Source = bmp;
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();
                _image.ToolTip = ex.Message;
            }
        }
    }
}
