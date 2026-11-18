using System;
using System.Collections.Generic;

namespace MathMenu.core
{
    /// <summary>
    /// Численное интегрирование (ЛР-3, вариант 17).
    /// Код оформлен в том же стиле, что и отлаженные программы ЛР-3:
    /// функция Func(x), локальные переменные a, b, n, h, Sum, Sum1, Sum2, x, i, Res,
    /// детерминированные циклы с управлением по аргументу.
    /// </summary>
    public static class Integration
    {
        /// <summary>Формула вычисления интеграла.</summary>
        public enum Quadrature
        {
            Left,
            Right,
            Trapezoid,
            Simpson
        }

        /// <summary>Способ задания шага.</summary>
        public enum StepControl
        {
            Fixed,      // постоянный шаг
            Runge,      // метод Рунге (автошаг)
            Adaptive    // адаптивный шаг
        }

        /// <summary>Подынтегральная функция (интеграл № 17 из ЛР-3).</summary>
        public static double Func(double x)
        {
            double F = (Math.Sqrt(0.6 * x + 1.7) / (2.1 * x + Math.Sqrt(0.7 * x + 1)));
            return F;
        }

        // ==================== методы с постоянным шагом ====================

        /// <summary>Метод левых прямоугольников.</summary>
        public static double LeftRectangle(double a, double b, int n)
        {
            double h = (b - a) / n;
            double Sum = 0;
            for (int i = 0; i < n; i++)
            {
                double x = a + h * i;
                Sum = Sum + Func(x);
            }
            return h * Sum;
        }

        /// <summary>Метод правых прямоугольников.</summary>
        public static double RightRectangle(double a, double b, int n)
        {
            double h = (b - a) / n;
            double Sum = 0;
            for (int i = 1; i <= n; i++)
            {
                double x = a + h * i;
                Sum = Sum + Func(x);
            }
            return h * Sum;
        }

        /// <summary>Метод трапеций.</summary>
        public static double Trapezoid(double a, double b, int n)
        {
            double h = (b - a) / n;
            double Sum = 0;
            for (int i = 1; i <= n - 1; i++)
            {
                double x = a + h * i;
                Sum += Func(x);
            }
            double Res = h * (((Func(a) + Func(b)) / 2) + Sum);
            return Res;
        }

        /// <summary>Метод парабол (метод Симпсона).</summary>
        public static double Simpson(double a, double b, int n)
        {
            if (n % 2 != 0)
            {
                n++;
            }
            double h = (b - a) / n;
            double Sum = Func(a) + Func(b);
            double Sum1 = 0;
            for (int i = 1; i < n; i += 2)
            {
                double x = a + h * i;
                Sum1 += Func(x);
            }
            double Sum2 = 0;
            for (int i = 2; i < n; i += 2)
            {
                double x = a + h * i;
                Sum2 += Func(x);
            }
            double Res = (h / 3) * (Sum + 4 * Sum1 + 2 * Sum2);
            return Res;
        }

        /// <summary>Вычисление интеграла выбранной формулой при постоянном шаге.</summary>
        public static double Calculate(Quadrature q, double a, double b, int n)
        {
            if (n < 1)
            {
                n = 1;
            }

            if (q == Quadrature.Left)
            {
                return LeftRectangle(a, b, n);
            }
            else if (q == Quadrature.Right)
            {
                return RightRectangle(a, b, n);
            }
            else if (q == Quadrature.Trapezoid)
            {
                return Trapezoid(a, b, n);
            }
            else
            {
                return Simpson(a, b, n);
            }
        }

        /// <summary>Порядок точности формулы (нужен для оценки по Рунге).</summary>
        public static int Order(Quadrature q)
        {
            if (q == Quadrature.Simpson)
            {
                return 4;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>Название формулы (для подписи в окне).</summary>
        public static string Name(Quadrature q)
        {
            if (q == Quadrature.Left)
            {
                return "метод левых прямоугольников";
            }
            else if (q == Quadrature.Right)
            {
                return "метод правых прямоугольников";
            }
            else if (q == Quadrature.Trapezoid)
            {
                return "метод трапеций";
            }
            else
            {
                return "метод Симпсона";
            }
        }

        /// <summary>Оценка погрешности по правилу Рунге.</summary>
        public static double RungeError(Quadrature q, double Ih, double Ihalf)
        {
            int p = Order(q);
            double Error = Math.Abs(Ihalf - Ih) / (Math.Pow(2, p) - 1);
            return Error;
        }

        /// <summary>Уточнённое значение по правилу Рунге.</summary>
        public static double RungeRefined(Quadrature q, double Ih, double Ihalf)
        {
            int p = Order(q);
            double Res = Ihalf + (Ihalf - Ih) / (Math.Pow(2, p) - 1);
            return Res;
        }

        /// <summary>Контрольное (эталонное) значение интеграла — Симпсон при большом n.</summary>
        public static double Reference(double a, double b)
        {
            return Simpson(a, b, 200000);
        }

        // ==================== метод Рунге (автошаг) ====================

        /// <summary>Строка таблицы итераций метода Рунге.</summary>
        public class RungeRow
        {
            public int K { get; set; }
            public int N { get; set; }
            public double H { get; set; }
            public double I { get; set; }
            public double Error { get; set; }
            public double Refined { get; set; }
        }

        /// <summary>
        /// Метод Рунге (автошаг): интеграл считается при n и 2n,
        /// шаг удваивается до тех пор, пока оценка погрешности по Рунге не станет меньше eps.
        /// </summary>
        public static double RungeAutoStep(Quadrature q, double a, double b, int n0, double eps,
                                           int maxIter, out List<RungeRow> rows, out bool converged)
        {
            int p = Order(q);
            double div = Math.Pow(2, p) - 1;
            rows = new List<RungeRow>();

            int n = n0;
            double I = Calculate(q, a, b, n);
            rows.Add(new RungeRow { K = 0, N = n, H = (b - a) / n, I = I, Error = 0, Refined = I });

            double Res = I;
            converged = false;

            for (int k = 1; k <= maxIter; k++)
            {
                n = n * 2;
                if (n > 4000000)
                {
                    break;
                }

                double Ihalf = Calculate(q, a, b, n);
                double Error = Math.Abs(Ihalf - I) / div;
                double Refined = Ihalf + (Ihalf - I) / div;

                rows.Add(new RungeRow
                {
                    K = k,
                    N = n,
                    H = (b - a) / n,
                    I = Ihalf,
                    Error = Error,
                    Refined = Refined
                });

                Res = Refined;
                if (Error <= eps)
                {
                    converged = true;
                    break;
                }

                I = Ihalf;
            }

            return Res;
        }

        // ==================== адаптивный шаг ====================

        /// <summary>Строка таблицы интервалов (шаг получился разным).</summary>
        public class AdaptiveRow
        {
            public int I { get; set; }
            public double X1 { get; set; }
            public double X2 { get; set; }
            public double H { get; set; }
            public double Value { get; set; }
            public double Error { get; set; }
        }

        private const int AdaptiveMaxRows = 4000;

        /// <summary>
        /// Адаптивный шаг: интервал дробится только там,
        /// где локальная оценка погрешности больше допустимой.
        /// </summary>
        public static double AdaptiveStep(Quadrature q, double a, double b, double eps,
                                          out List<AdaptiveRow> rows, out double totalError)
        {
            rows = new List<AdaptiveRow>();

            double value;

            if (q == Quadrature.Simpson)
            {
                double whole = SimpsonRange(a, b);
                value = AdaptSimpson(a, b, whole, eps, 30, rows);
            }
            else
            {
                double whole = TrapezoidRange(a, b);
                value = AdaptTrapezoid(a, b, whole, eps, 30, rows);
            }

            totalError = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                totalError += rows[i].Error;
                rows[i].I = i + 1;
            }

            return value;
        }

        private static double SimpsonRange(double a, double b)
        {
            double m = (a + b) / 2;
            double Res = (b - a) / 6 * (Func(a) + 4 * Func(m) + Func(b));
            return Res;
        }

        private static double TrapezoidRange(double a, double b)
        {
            double Res = (b - a) / 2 * (Func(a) + Func(b));
            return Res;
        }

        private static double AdaptSimpson(double a, double b, double whole, double eps,
                                           int depth, List<AdaptiveRow> rows)
        {
            double m = (a + b) / 2;
            double left = SimpsonRange(a, m);
            double right = SimpsonRange(m, b);
            double delta = left + right - whole;

            if (depth <= 0 || rows.Count >= AdaptiveMaxRows || Math.Abs(delta) <= 15 * eps)
            {
                double Res = left + right + delta / 15;
                rows.Add(new AdaptiveRow
                {
                    X1 = a,
                    X2 = b,
                    H = b - a,
                    Value = Res,
                    Error = Math.Abs(delta) / 15
                });
                return Res;
            }

            return AdaptSimpson(a, m, left, eps / 2, depth - 1, rows)
                 + AdaptSimpson(m, b, right, eps / 2, depth - 1, rows);
        }

        private static double AdaptTrapezoid(double a, double b, double whole, double eps,
                                             int depth, List<AdaptiveRow> rows)
        {
            double m = (a + b) / 2;
            double left = TrapezoidRange(a, m);
            double right = TrapezoidRange(m, b);
            double delta = left + right - whole;

            if (depth <= 0 || rows.Count >= AdaptiveMaxRows || Math.Abs(delta) <= 3 * eps)
            {
                double Res = left + right + delta / 3;
                rows.Add(new AdaptiveRow
                {
                    X1 = a,
                    X2 = b,
                    H = b - a,
                    Value = Res,
                    Error = Math.Abs(delta) / 3
                });
                return Res;
            }

            return AdaptTrapezoid(a, m, left, eps / 2, depth - 1, rows)
                 + AdaptTrapezoid(m, b, right, eps / 2, depth - 1, rows);
        }
    }
}
