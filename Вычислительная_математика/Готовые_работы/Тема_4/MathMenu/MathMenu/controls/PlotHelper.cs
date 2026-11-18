using ScottPlot;
using ScottPlot.Plottables;

namespace MathMenu.controls
{
    /// <summary>
    /// Общее оформление графиков ScottPlot: подписи осей, легенда, сетка, шрифт с кириллицей.
    /// </summary>
    public static class PlotHelper
    {
        public static readonly Color EulerColor = Color.FromHex("#C00000");
        public static readonly Color RungeColor = Color.FromHex("#1F5FBF");
        public static readonly Color ExactColor = Color.FromHex("#1E8E3E");
        public static readonly Color CurveColor = Color.FromHex("#404040");
        public static readonly Color GridColor = Color.FromHex("#DCDCDC");

        public static Plot Create(string title, string xLabel, string yLabel)
        {
            Plot plt = new Plot();

            plt.Font.Automatic();
            plt.Axes.Title.Label.Text = title;
            plt.Axes.Title.Label.FontSize = 15;
            plt.Axes.Title.Label.Bold = true;
            plt.Axes.Bottom.Label.Text = xLabel;
            plt.Axes.Bottom.Label.FontSize = 12;
            plt.Axes.Left.Label.Text = yLabel;
            plt.Axes.Left.Label.FontSize = 12;

            plt.Legend.IsVisible = true;
            plt.Legend.Alignment = Alignment.UpperLeft;
            plt.Legend.FontSize = 10;
            plt.Legend.BackgroundColor = Color.FromHex("#FFFFFF");
            plt.Legend.OutlineColor = Color.FromHex("#B0B0B0");

            plt.Grid.MajorLineColor = GridColor;
            plt.Grid.MinorLineColor = Color.FromHex("#EFEFEF");

            return plt;
        }

        /// <summary>Линия без маркеров.</summary>
        public static void AsLine(Scatter s, Color color, float width)
        {
            s.Color = color;
            s.LineWidth = width;
            s.MarkerSize = 0;
        }

        /// <summary>Линия с маркерами (точки таблицы).</summary>
        public static void AsPoints(Scatter s, Color color, float width, float marker)
        {
            s.Color = color;
            s.LineWidth = width;
            s.MarkerSize = marker;
        }
    }
}
