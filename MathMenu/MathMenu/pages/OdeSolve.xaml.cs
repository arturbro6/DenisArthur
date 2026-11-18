using System;
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
    /// Логика взаимодействия для OdeSolve.xaml
    /// Численное решение ОДУ 1-го порядка, ОДУ 2-го порядка и системы трёх уравнений
    /// методами Эйлера и Рунге-Кутта (контрольные примеры 1, 2, 3).
    /// </summary>
    public partial class OdeSolve : Page
    {
        private OdeType _type = OdeType.FirstOrder;
        private OdeMethod _method = OdeMethod.Euler;
        private bool _ready = false;
        private PlotCanvas _plot;

        private OdeSolution _euler;
        private OdeSolution _rk;
        private int _n = 10;
        private double _hReal = 0.1;

        private double _rkExactError = 0;
        private int _bestMult = 0;
        private double _bestDiff = 0;

        public OdeSolve()
        {
            InitializeComponent();
            _plot = new PlotCanvas(PlotImage);
            Init();
        }

        public OdeSolve(int type, int method)
        {
            InitializeComponent();
            _plot = new PlotCanvas(PlotImage);
            _type = (OdeType)type;
            _method = (OdeMethod)method;
            Init();
        }

        private void Init()
        {
            TypeBox.SelectedIndex = (int)_type;
            MethodBox.SelectedIndex = (int)_method;
            LoadDefaults();
            _ready = true;
            UpdateControls();
            Calculate();
        }

        // ==================== начальные данные ====================

        private void LoadDefaults()
        {
            double x0;
            double xEnd;
            double h;
            double u0;
            double v0;
            double w0;

            Ode.Defaults(_type, out x0, out xEnd, out h, out u0, out v0, out w0);

            X0Box.Text = Num(x0);
            XEndBox.Text = Num(xEnd);
            HBox.Text = Num(h);
            U0Box.Text = Num(u0);
            V0Box.Text = Num(v0);
            W0Box.Text = Num(w0);
        }

        private void ResetButt_Click(object sender, RoutedEventArgs e)
        {
            LoadDefaults();
            Calculate();
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ready || TypeBox.SelectedIndex < 0)
            {
                return;
            }

            _type = (OdeType)TypeBox.SelectedIndex;
            LoadDefaults();
            UpdateControls();
            Calculate();
        }

        private void MethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ready || MethodBox.SelectedIndex < 0)
            {
                return;
            }

            _method = (OdeMethod)MethodBox.SelectedIndex;
            UpdateControls();
            Calculate();
        }

        private void UpdateControls()
        {
            L1.Content = Ode.MethodName(_method);
            EquationText.Text = Ode.Equation(_type);

            bool system = (_type == OdeType.System3);
            bool second = (_type == OdeType.SecondOrder);

            if (system)
            {
                X0Label.Content = "t₀:";
                XEndLabel.Content = "конечное значение t:";
                U0Label.Content = "x₀:";
                V0Label.Content = "y₀:";
                W0Label.Content = "z₀:";
            }
            else
            {
                X0Label.Content = "x₀:";
                XEndLabel.Content = "конечное значение x:";
                U0Label.Content = "y₀:";
                V0Label.Content = "y′₀:";
            }

            V0Label.Visibility = (second || system) ? Visibility.Visible : Visibility.Collapsed;
            V0Box.Visibility = (second || system) ? Visibility.Visible : Visibility.Collapsed;
            W0Label.Visibility = system ? Visibility.Visible : Visibility.Collapsed;
            W0Box.Visibility = system ? Visibility.Visible : Visibility.Collapsed;
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
            double x0;
            double xEnd;
            double h;
            double u0;
            double v0;
            double w0;

            double dx0;
            double dxEnd;
            double dh;
            double du0;
            double dv0;
            double dw0;
            Ode.Defaults(_type, out dx0, out dxEnd, out dh, out du0, out dv0, out dw0);

            if (!TryParseDouble(X0Box.Text, out x0)) x0 = dx0;
            if (!TryParseDouble(XEndBox.Text, out xEnd)) xEnd = dxEnd;
            if (!TryParseDouble(HBox.Text, out h)) h = dh;
            if (!TryParseDouble(U0Box.Text, out u0)) u0 = du0;
            if (!TryParseDouble(V0Box.Text, out v0)) v0 = dv0;
            if (!TryParseDouble(W0Box.Text, out w0)) w0 = dw0;

            if (xEnd <= x0)
            {
                ShowError("конечное значение должно быть больше начального");
                return;
            }
            if (h <= 0 || h > (xEnd - x0))
            {
                ShowError("шаг h должен быть положительным и не больше длины отрезка");
                return;
            }

            int n = (int)Math.Round((xEnd - x0) / h, MidpointRounding.AwayFromZero);
            if (n < 1)
            {
                n = 1;
            }
            if (n > 100000)
            {
                ShowError("слишком мелкий шаг: число разбиений больше 100000");
                return;
            }

            _n = n;
            _hReal = (xEnd - x0) / n;

            try
            {
                _euler = Ode.Solve(_type, OdeMethod.Euler, x0, xEnd, n, u0, v0, w0);
                _rk = Ode.Solve(_type, OdeMethod.RungeKutta, x0, xEnd, n, u0, v0, w0);

                _rkExactError = 0;
                if (_type == OdeType.FirstOrder)
                {
                    for (int i = 0; i <= n; i++)
                    {
                        double e = Math.Abs(_rk.U[0][i] - Ode.Exact1(_rk.X[i]));
                        if (e > _rkExactError)
                        {
                            _rkExactError = e;
                        }
                    }
                }

                FillMainTable();
                FillComparisonTable(x0, xEnd, u0, v0, w0);
                DrawPlot();
                UpdateInfo();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void FillMainTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("№");
            table.Columns.Add(_type == OdeType.System3 ? "t" : "x");

            if (_type == OdeType.FirstOrder)
            {
                table.Columns.Add("y (Эйлер)");
                table.Columns.Add("y (Рунге-Кутта)");
                table.Columns.Add("|Δ|");
                table.Columns.Add("y точное");
                table.Columns.Add("погрешность РК");
            }
            else if (_type == OdeType.SecondOrder)
            {
                table.Columns.Add("y (Эйлер)");
                table.Columns.Add("y (Рунге-Кутта)");
                table.Columns.Add("y′ (Эйлер)");
                table.Columns.Add("y′ (Рунге-Кутта)");
                table.Columns.Add("|Δy|");
            }
            else
            {
                table.Columns.Add("x (Эйлер)");
                table.Columns.Add("x (Рунге-Кутта)");
                table.Columns.Add("y (Эйлер)");
                table.Columns.Add("y (Рунге-Кутта)");
                table.Columns.Add("z (Эйлер)");
                table.Columns.Add("z (Рунге-Кутта)");
            }

            int stride = 1;
            while (_n / stride > 5000)
            {
                stride = stride * 2;
            }

            for (int i = 0; i <= _n; i = i + stride)
            {
                AddMainRow(table, i);
            }
            if (_n % stride != 0)
            {
                AddMainRow(table, _n);
            }

            GridBinder.Bind(MainGrid, table);
        }

        private void AddMainRow(DataTable table, int i)
        {
            string number = i.ToString(CultureInfo.CurrentCulture);
            double x = _euler.X[i];

            if (_type == OdeType.FirstOrder)
            {
                double ye = _euler.U[0][i];
                double yr = _rk.U[0][i];
                double exact = Ode.Exact1(x);
                table.Rows.Add(number, F(x), F(ye), F(yr), E(Math.Abs(ye - yr)), F(exact),
                               E(Math.Abs(yr - exact)));
            }
            else if (_type == OdeType.SecondOrder)
            {
                double ye = _euler.U[0][i];
                double yr = _rk.U[0][i];
                table.Rows.Add(number, F(x), F(ye), F(yr),
                               F(_euler.U[1][i]), F(_rk.U[1][i]), E(Math.Abs(ye - yr)));
            }
            else
            {
                table.Rows.Add(number, F(x),
                               F(_euler.U[0][i]), F(_rk.U[0][i]),
                               F(_euler.U[1][i]), F(_rk.U[1][i]),
                               F(_euler.U[2][i]), F(_rk.U[2][i]));
            }
        }

        /// <summary>
        /// Таблица сравнения: как метод Эйлера приближается к Рунге-Кутту
        /// при уменьшении шага (п. 3.4 / 7.1 задания).
        /// </summary>
        private void FillComparisonTable(double x0, double xEnd, double u0, double v0, double w0)
        {
            DataTable table = new DataTable();
            table.Columns.Add("вычисления");
            table.Columns.Add("n");
            table.Columns.Add("шаг h");
            table.Columns.Add("макс отклонение от РК");

            _bestMult = 0;
            _bestDiff = 0;

            double tol = 0.0001;

            int[] mults = new int[] { 10, 100, 1000 };
            for (int k = 0; k < mults.Length; k++)
            {
                int mult = mults[k];
                if ((long)_n * mult > 500000)
                {
                    continue;
                }

                OdeSolution fine = Ode.Solve(_type, OdeMethod.Euler, x0, xEnd, _n * mult, u0, v0, w0);
                double diff = Ode.MaxDifference(_rk, fine, mult);

                table.Rows.Add("метод Эйлера", (_n * mult).ToString(CultureInfo.CurrentCulture),
                               E(_hReal / mult), E(diff));

                if (_bestMult == 0 && diff <= tol)
                {
                    _bestMult = mult;
                    _bestDiff = diff;
                }
            }

            OdeSolution rkFine = Ode.Solve(_type, OdeMethod.RungeKutta, x0, xEnd, _n * 2, u0, v0, w0);
            double rkDiff = Ode.MaxDifference(_rk, rkFine, 2);
            table.Rows.Add("метод Рунге-Кутта (контроль)", (_n * 2).ToString(CultureInfo.CurrentCulture),
                           E(_hReal / 2), E(rkDiff));

            GridBinder.Bind(CmpGrid, table);
        }

        private void UpdateInfo()
        {
            double diffMethods = Ode.MaxDifference(_rk, _euler, 1);

            string s = "узлов: " + (_n + 1).ToString(CultureInfo.CurrentCulture) +
                       ",   шаг h = " + Num(_hReal) +
                       ",   выбрано: " + Ode.MethodName(_method) +
                       "\nмакс. расхождение решений Эйлера и Рунге-Кутта при этом шаге: " + E(diffMethods);

            if (_type == OdeType.FirstOrder)
            {
                s += "\nмакс. погрешность Рунге-Кутта относительно точного решения y = e^(x − x²/2): " + E(_rkExactError);
            }

            if (_bestMult > 0)
            {
                s += "\nрекомендуемый шаг Эйлера (расхождение ≤ 1E-04): h / " +
                     _bestMult.ToString(CultureInfo.CurrentCulture) +
                     ", т.е. n = " + (_n * _bestMult).ToString(CultureInfo.CurrentCulture) +
                     " (расхождение " + E(_bestDiff) + ")";
            }
            else
            {
                s += "\nдля расхождения ≤ 1E-04 шаг Эйлера нужно уменьшить более чем в 1000 раз";
            }

            InfoLabel.Text = s;
        }

        // ==================== график ====================

        private void DrawPlot()
        {
            string title;
            string xLabel;
            string yLabel;

            if (_type == OdeType.FirstOrder)
            {
                title = "y′ = y·(1 − x),   h = " + Num(_hReal);
                xLabel = "x";
                yLabel = "y";
            }
            else if (_type == OdeType.SecondOrder)
            {
                title = "y″ + y′/x + y = 0,   h = " + Num(_hReal);
                xLabel = "x";
                yLabel = "y";
            }
            else
            {
                title = "Система трёх уравнений,   h = " + Num(_hReal);
                xLabel = "t";
                yLabel = "x, y, z";
            }

            ScottPlot.Plot plt = PlotHelper.Create(title, xLabel, yLabel);

            bool eulerChosen = (_method == OdeMethod.Euler);

            if (_type == OdeType.FirstOrder)
            {
                AddCurve(plt, _euler.X, _euler.U[0], PlotHelper.EulerColor, eulerChosen,
                         "y (Эйлер)");
                AddCurve(plt, _rk.X, _rk.U[0], PlotHelper.RungeColor, !eulerChosen,
                         "y (Рунге-Кутта)");

                double[] ex = new double[_rk.X.Length];
                for (int i = 0; i < ex.Length; i++)
                {
                    ex[i] = Ode.Exact1(_rk.X[i]);
                }
                ScottPlot.Plottables.Scatter s = plt.Add.Scatter(_rk.X, ex);
                PlotHelper.AsLine(s, PlotHelper.ExactColor, 1.5f);
                s.LinePattern = ScottPlot.LinePattern.Dashed;
                s.LegendText = "y точное";
            }
            else if (_type == OdeType.SecondOrder)
            {
                AddCurve(plt, _euler.X, _euler.U[0], PlotHelper.EulerColor, eulerChosen,
                         "y (Эйлер)");
                AddCurve(plt, _rk.X, _rk.U[0], PlotHelper.RungeColor, !eulerChosen,
                         "y (Рунге-Кутта)");
            }
            else
            {
                ScottPlot.Color cx = ScottPlot.Color.FromHex("#C00000");
                ScottPlot.Color cy = ScottPlot.Color.FromHex("#1F5FBF");
                ScottPlot.Color cz = ScottPlot.Color.FromHex("#1E8E3E");

                AddCurve(plt, _euler.X, _euler.U[0], cx, eulerChosen, "x (Эйлер)");
                AddCurve(plt, _rk.X, _rk.U[0], cx, !eulerChosen, "x (Рунге-Кутта)");
                AddCurve(plt, _euler.X, _euler.U[1], cy, eulerChosen, "y (Эйлер)");
                AddCurve(plt, _rk.X, _rk.U[1], cy, !eulerChosen, "y (Рунге-Кутта)");
                AddCurve(plt, _euler.X, _euler.U[2], cz, eulerChosen, "z (Эйлер)");
                AddCurve(plt, _rk.X, _rk.U[2], cz, !eulerChosen, "z (Рунге-Кутта)");

                // шесть кривых — легенду выносим за пределы поля графика
                plt.ShowLegend(ScottPlot.Edge.Right);
            }

            plt.Axes.AutoScale();
            plt.Axes.Margins(0.05, 0.12);
            _plot.Show(plt);
        }

        private void AddCurve(ScottPlot.Plot plt, double[] xs, double[] ys,
                              ScottPlot.Color color, bool chosen, string legend)
        {
            ScottPlot.Plottables.Scatter s = plt.Add.Scatter(xs, ys);
            s.Color = color;
            s.LineWidth = chosen ? 2.5f : 1.5f;
            s.MarkerSize = chosen ? 3 : 0;
            s.LinePattern = chosen ? ScottPlot.LinePattern.Solid : ScottPlot.LinePattern.Dashed;
            s.LegendText = legend;
        }

        // ==================== вспомогательное ====================

        private void ShowError(string message)
        {
            MainGrid.ItemsSource = null;
            CmpGrid.ItemsSource = null;
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

        private static string Num(double v)
        {
            return v.ToString("0.0####", CultureInfo.CurrentCulture);
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
