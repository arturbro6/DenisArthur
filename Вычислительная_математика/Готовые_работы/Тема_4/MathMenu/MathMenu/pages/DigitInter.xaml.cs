using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MathMenu.controls;
using MathMenu.core;

namespace MathMenu.pages
{
    /// <summary>
    /// Логика взаимодействия для DigitInter.xaml
    /// Численное интегрирование: постоянный шаг (4 формулы),
    /// переменный шаг (метод Рунге — автошаг, адаптивный шаг).
    /// </summary>
    public partial class DigitInter : Page
    {
        private int _step = 0;                                  // 0 — постоянный, 1 — переменный
        private int _method = 3;                                // индекс в списке методов
        private Integration.Quadrature _formula = Integration.Quadrature.Simpson;
        private bool _ready = false;
        private PlotCanvas _plot;

        private const double DefA = 1.2;
        private const double DefB = 2.0;
        private const int DefN = 10000;
        private const int DefN0 = 4;
        private const double DefEps = 0.00000001;

        public DigitInter()
        {
            InitializeComponent();
            _plot = new PlotCanvas(PlotImage);
            LoadDefaults();
            _step = 0;
            _method = 3;
            _formula = Integration.Quadrature.Simpson;
            FillCombos();
            _ready = true;
            UpdateControls();
            Calculate();
        }

        public DigitInter(int step, int method)
        {
            InitializeComponent();
            _plot = new PlotCanvas(PlotImage);
            LoadDefaults();

            _step = step;
            _method = method;
            if (_step == 0)
            {
                _formula = (Integration.Quadrature)method;
            }
            else
            {
                _formula = Integration.Quadrature.Simpson;
                NBox.Text = DefN0.ToString(CultureInfo.CurrentCulture);
            }

            FillCombos();
            _ready = true;
            UpdateControls();
            Calculate();
        }

        private void LoadDefaults()
        {
            LowerLim.Text = DefA.ToString("0.0###", CultureInfo.CurrentCulture);
            UpperLim.Text = DefB.ToString("0.0###", CultureInfo.CurrentCulture);
            NBox.Text = DefN.ToString(CultureInfo.CurrentCulture);
            EpsBox.Text = "1E-08";
        }

        private void FillCombos()
        {
            StepBox.SelectedIndex = _step;
            FillMethods();
            MethodBox.SelectedIndex = _method;
            FormulaBox.SelectedIndex = (_formula == Integration.Quadrature.Trapezoid) ? 0 : 1;
        }

        private void FillMethods()
        {
            MethodBox.Items.Clear();
            if (_step == 0)
            {
                MethodBox.Items.Add("метод левых прямоугольников");
                MethodBox.Items.Add("метод правых прямоугольников");
                MethodBox.Items.Add("метод трапеций");
                MethodBox.Items.Add("метод Симпсона");
            }
            else
            {
                MethodBox.Items.Add("метод Рунге (автошаг)");
                MethodBox.Items.Add("адаптивный шаг");
            }
        }

        // ==================== обработчики выбора ====================

        private void StepBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ready || StepBox.SelectedIndex < 0)
            {
                return;
            }

            _step = StepBox.SelectedIndex;
            _method = 0;
            if (_step == 0)
            {
                NBox.Text = DefN.ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                NBox.Text = DefN0.ToString(CultureInfo.CurrentCulture);
            }
            FillMethods();
            MethodBox.SelectedIndex = 0;
            UpdateControls();
            Calculate();
        }

        private void MethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ready || MethodBox.SelectedIndex < 0)
            {
                return;
            }

            _method = MethodBox.SelectedIndex;
            if (_step == 0)
            {
                _formula = (Integration.Quadrature)_method;
            }
            UpdateControls();
            Calculate();
        }

        private void FormulaBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ready || FormulaBox.SelectedIndex < 0)
            {
                return;
            }

            if (FormulaBox.SelectedIndex == 0)
            {
                _formula = Integration.Quadrature.Trapezoid;
            }
            else
            {
                _formula = Integration.Quadrature.Simpson;
            }

            UpdateControls();
            Calculate();
        }

        private void UpdateControls()
        {
            bool variable = (_step == 1);

            FormulaLabel.Visibility = variable ? Visibility.Visible : Visibility.Collapsed;
            FormulaBox.Visibility = variable ? Visibility.Visible : Visibility.Collapsed;
            EpsLabel.Visibility = variable ? Visibility.Visible : Visibility.Collapsed;
            EpsBox.Visibility = variable ? Visibility.Visible : Visibility.Collapsed;

            bool showN = !variable || _method == 0;
            NLabel.Visibility = showN ? Visibility.Visible : Visibility.Collapsed;
            NBox.Visibility = showN ? Visibility.Visible : Visibility.Collapsed;

            if (variable && _method == 0)
            {
                NLabel.Content = "начальное число разбиений n0:";
            }
            else
            {
                NLabel.Content = "число разбиений n:";
            }

            if (!variable)
            {
                L1.Content = Integration.Name((Integration.Quadrature)_method);
            }
            else
            {
                string mode = (_method == 0) ? "метод Рунге (автошаг)" : "адаптивный шаг";
                L1.Content = mode + " — " + Integration.Name(_formula);
            }
        }

        // ==================== вычисление ====================

        private void CalcButt_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        /// <summary>Enter в любом поле ввода — пересчитать.</summary>
        private void Inputs_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Calculate();
                e.Handled = true;
            }
        }

        private void Calculate()
        {
            double a;
            double b;
            int n;
            double eps;

            if (!TryParseDouble(LowerLim.Text, out a) || !TryParseDouble(UpperLim.Text, out b))
            {
                ShowError("проверьте пределы интегрирования");
                return;
            }
            if (b <= a)
            {
                ShowError("верхний предел должен быть больше нижнего");
                return;
            }
            if (!TryParseInt(NBox.Text, out n))
            {
                n = DefN;
            }
            if (!TryParseDouble(EpsBox.Text, out eps) || eps <= 0)
            {
                eps = DefEps;
            }
            if (n < 1)
            {
                n = 1;
            }
            if (n > 200000)
            {
                n = 200000;
                NBox.Text = n.ToString(CultureInfo.CurrentCulture);
            }

            try
            {
                if (_step == 0)
                {
                    CalculateFixed(a, b, n);
                }
                else if (_method == 0)
                {
                    CalculateRunge(a, b, n, eps);
                }
                else
                {
                    CalculateAdaptive(a, b, eps);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Постоянный шаг: считаем I(n) и I(2n), оценка погрешности по Рунге.</summary>
        private void CalculateFixed(double a, double b, int n)
        {
            Integration.Quadrature q = (Integration.Quadrature)_method;
            double refValue = Integration.Reference(a, b);

            DataTable table = new DataTable();
            table.Columns.Add("метод");
            table.Columns.Add("I(n)");
            table.Columns.Add("I(2n)");
            table.Columns.Add("оценка по Рунге");
            table.Columns.Add("уточнённое");
            table.Columns.Add("отклонение от эталона");

            double In = 0;
            double err = 0;
            double refined = 0;

            for (int k = 0; k < 4; k++)
            {
                Integration.Quadrature qk = (Integration.Quadrature)k;
                double Ih = Integration.Calculate(qk, a, b, n);
                double Ihalf = Integration.Calculate(qk, a, b, n * 2);
                double e = Integration.RungeError(qk, Ih, Ihalf);
                double r = Integration.RungeRefined(qk, Ih, Ihalf);

                table.Rows.Add(Integration.Name(qk), F(Ih), F(Ihalf), E(e), F(r),
                               E(Math.Abs(Ih - refValue)));

                if (k == (int)q)
                {
                    In = Ih;
                    err = e;
                    refined = r;
                }
            }

            GridBinder.Bind(ResultsGrid, table);
            ResultLabel.Content = "I = " + F(In) + "   (уточнённое по Рунге: " + F(refined) + ")";
            InfoLabel.Text = "метод: " + Integration.Name(q) + ",   n = " + n.ToString(CultureInfo.CurrentCulture) +
                             ",   h = " + E((b - a) / n) +
                             ",   оценка погрешности по Рунге: " + E(err) +
                             "\nэталон (метод Симпсона, n = 200000): " + F(refValue) +
                             ",   |I − эталон| = " + E(Math.Abs(In - refValue));

            DrawPlot(a, b, 0, null);
        }

        /// <summary>Переменный шаг: метод Рунге (автошаг).</summary>
        private void CalculateRunge(double a, double b, int n0, double eps)
        {
            List<Integration.RungeRow> rows;
            bool converged;
            double value = Integration.RungeAutoStep(_formula, a, b, n0, eps, 20, out rows, out converged);
            double refValue = Integration.Reference(a, b);

            DataTable table = new DataTable();
            table.Columns.Add("k");
            table.Columns.Add("n");
            table.Columns.Add("h");
            table.Columns.Add("I(n)");
            table.Columns.Add("оценка по Рунге");
            table.Columns.Add("уточнённое");

            for (int i = 0; i < rows.Count; i++)
            {
                Integration.RungeRow r = rows[i];
                table.Rows.Add(r.K.ToString(CultureInfo.CurrentCulture),
                               r.N.ToString(CultureInfo.CurrentCulture),
                               E(r.H),
                               F(r.I),
                               (i == 0 ? "—" : E(r.Error)),
                               F(r.Refined));
            }

            GridBinder.Bind(ResultsGrid, table);

            Integration.RungeRow last = rows[rows.Count - 1];
            ResultLabel.Content = "I = " + F(value);
            InfoLabel.Text = "формула: " + Integration.Name(_formula) +
                             ",   eps = " + E(eps) +
                             ",   итераций: " + last.K.ToString(CultureInfo.CurrentCulture) +
                             ",   последнее n = " + last.N.ToString(CultureInfo.CurrentCulture) +
                             "\n" + (converged
                                 ? "точность достигнута: оценка по Рунге " + E(last.Error) + " ≤ eps"
                                 : "предел числа итераций: оценка по Рунге " + E(last.Error)) +
                             ",   |I − эталон| = " + E(Math.Abs(value - refValue));

            DrawPlot(a, b, 1, null);
        }

        /// <summary>Переменный шаг: адаптивный алгоритм.</summary>
        private void CalculateAdaptive(double a, double b, double eps)
        {
            List<Integration.AdaptiveRow> rows;
            double totalError;
            double value = Integration.AdaptiveStep(_formula, a, b, eps, out rows, out totalError);
            double refValue = Integration.Reference(a, b);

            double hMin = double.MaxValue;
            double hMax = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].H < hMin)
                {
                    hMin = rows[i].H;
                }
                if (rows[i].H > hMax)
                {
                    hMax = rows[i].H;
                }
            }

            DataTable table = new DataTable();
            table.Columns.Add("№");
            table.Columns.Add("x начало");
            table.Columns.Add("x конец");
            table.Columns.Add("шаг h");
            table.Columns.Add("вклад");
            table.Columns.Add("локальная оценка");

            for (int i = 0; i < rows.Count; i++)
            {
                Integration.AdaptiveRow r = rows[i];
                table.Rows.Add(r.I.ToString(CultureInfo.CurrentCulture),
                               F(r.X1),
                               F(r.X2),
                               E(r.H),
                               F(r.Value),
                               E(r.Error));
            }

            GridBinder.Bind(ResultsGrid, table);

            ResultLabel.Content = "I = " + F(value);
            InfoLabel.Text = "формула: " + Integration.Name(_formula) +
                             ",   eps = " + E(eps) +
                             ",   интервалов: " + rows.Count.ToString(CultureInfo.CurrentCulture) +
                             ",   шаг от " + E(hMin) + " до " + E(hMax) + " (переменный)" +
                             "\nсуммарная оценка погрешности: " + E(totalError) +
                             ",   |I − эталон| = " + E(Math.Abs(value - refValue));

            DrawPlot(a, b, 2, rows);
        }

        // ==================== график ====================

        private void DrawPlot(double a, double b, int mode, List<Integration.AdaptiveRow> rows)
        {
            ScottPlot.Plot plt = PlotHelper.Create("f(x) = √(0,6x + 1,7) / (2,1x + √(0,7x + 1))", "x", "f(x)");

            int count = 400;
            double[] xs = new double[count + 1];
            double[] ys = new double[count + 1];
            for (int i = 0; i <= count; i++)
            {
                double x = a + (b - a) * i / count;
                xs[i] = x;
                ys[i] = Integration.Func(x);
            }

            ScottPlot.Plottables.Scatter curve = plt.Add.Scatter(xs, ys);
            PlotHelper.AsLine(curve, PlotHelper.CurveColor, 2);
            curve.LegendText = "f(x)";

            if (mode == 0)
            {
                // узлы равномерной сетки (если их не слишком много)
                int n = 0;
                if (int.TryParse(NBox.Text, out n) && n > 0 && n <= 60)
                {
                    double[] nx = new double[n + 1];
                    double[] ny = new double[n + 1];
                    for (int i = 0; i <= n; i++)
                    {
                        double x = a + (b - a) * i / n;
                        nx[i] = x;
                        ny[i] = Integration.Func(x);
                    }
                    ScottPlot.Plottables.Scatter nodes = plt.Add.ScatterPoints(nx, ny);
                    PlotHelper.AsPoints(nodes, PlotHelper.RungeColor, 0, 6);
                    nodes.LegendText = "узлы сетки";
                }
            }
            else if (mode == 2 && rows != null)
            {
                // границы интервалов адаптивной сетки (показываем, если их немного)
                int m = rows.Count;
                if (m > 0 && m <= 60)
                {
                    double[] nx = new double[m + 1];
                    double[] ny = new double[m + 1];
                    nx[0] = rows[0].X1;
                    ny[0] = Integration.Func(nx[0]);
                    for (int i = 0; i < m; i++)
                    {
                        nx[i + 1] = rows[i].X2;
                        ny[i + 1] = Integration.Func(nx[i + 1]);
                    }
                    ScottPlot.Plottables.Scatter nodes = plt.Add.ScatterPoints(nx, ny);
                    PlotHelper.AsPoints(nodes, PlotHelper.RungeColor, 0, 6);
                    nodes.LegendText = "границы интервалов";
                }
            }

            plt.Axes.AutoScale();
            plt.Axes.Margins(0, 0.08);
            _plot.Show(plt);
        }

        // ==================== вспомогательное ====================

        private void ShowError(string message)
        {
            ResultsGrid.ItemsSource = null;
            ResultLabel.Content = "I = —";
            InfoLabel.Text = message;
            _plot.Clear();
        }

        private static string F(double v)
        {
            return v.ToString("F6", CultureInfo.CurrentCulture);
        }

        private static string E(double v)
        {
            return v.ToString("0.00E+0", CultureInfo.CurrentCulture);
        }

        private static bool TryParseDouble(string s, out double v)
        {
            v = 0;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            s = s.Trim().Replace(',', '.');
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }

        private static bool TryParseInt(string s, out int v)
        {
            v = 0;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            return int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out v);
        }

        private void BackButt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void PlotArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_plot != null)
            {
                _plot.Resize();
            }
        }
    }
}
