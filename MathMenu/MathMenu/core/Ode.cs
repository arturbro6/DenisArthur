using System;

namespace MathMenu.core
{
    /// <summary>Тип задачи.</summary>
    public enum OdeType
    {
        FirstOrder,     // ОДУ 1-го порядка
        SecondOrder,    // ОДУ 2-го порядка
        System3         // система трёх уравнений
    }

    /// <summary>Численный метод.</summary>
    public enum OdeMethod
    {
        Euler,          // метод Эйлера
        RungeKutta      // метод Рунге-Кутта
    }

    /// <summary>Результат решения: узлы X и значения функций U[компонента][узел].</summary>
    public class OdeSolution
    {
        public double[] X;
        public double[][] U;
        public string[] Names;

        public int N
        {
            get { return X.Length - 1; }
        }

        public double H
        {
            get
            {
                if (X.Length > 1)
                {
                    return X[1] - X[0];
                }
                else
                {
                    return 0;
                }
            }
        }
    }

    /// <summary>
    /// Численное решение дифференциальных уравнений (метод Эйлера и метод Рунге-Кутта).
    /// Код оформлен в том же стиле, что и отлаженные программы ЛР-3:
    /// отдельная функция правой части, рекуррентные выражения лекции,
    /// детерминированный цикл с управлением по аргументу.
    /// </summary>
    public static class Ode
    {
        // ==================== правые части уравнений ====================

        /// <summary>ОДУ 1-го порядка (контрольный пример 1): y' = y*(1 - x).</summary>
        public static double Func1(double x, double y)
        {
            double F = y * (1 - x);
            return F;
        }

        /// <summary>Точное решение примера 1: y = exp(x - x^2/2).</summary>
        public static double Exact1(double x)
        {
            double F = Math.Exp(x - x * x / 2);
            return F;
        }

        /// <summary>ОДУ 2-го порядка (контрольный пример 2): y'' + y'/x + y = 0, y' = z.</summary>
        public static double Func2y(double x, double y, double z)
        {
            double F = z;
            return F;
        }

        /// <summary>Вторая функция системы: z' = -z/x - y.</summary>
        public static double Func2z(double x, double y, double z)
        {
            double F = -z / x - y;
            return F;
        }

        /// <summary>Система трёх уравнений (контрольный пример 3): dx/dt = -2x + 5z.</summary>
        public static double Func3x(double t, double x, double y, double z)
        {
            double F = -2 * x + 5 * z;
            return F;
        }

        /// <summary>dy/dt = sin(t - 1)*x - y + 3z.</summary>
        public static double Func3y(double t, double x, double y, double z)
        {
            double F = Math.Sin(t - 1) * x - y + 3 * z;
            return F;
        }

        /// <summary>dz/dt = -x + 2z.</summary>
        public static double Func3z(double t, double x, double y, double z)
        {
            double F = -x + 2 * z;
            return F;
        }

        // ==================== ОДУ 1-го порядка ====================

        /// <summary>Метод Эйлера: y(i+1) = y(i) + h*f(x(i), y(i)); x(i+1) = x(i) + h.</summary>
        public static void Euler1(double x0, double y0, double h, int n, double[] X, double[] Y)
        {
            double x = x0;
            double y = y0;
            X[0] = x;
            Y[0] = y;
            for (int i = 1; i <= n; i++)
            {
                y = y + h * Func1(x, y);
                x = x + h;
                X[i] = x;
                Y[i] = y;
            }
        }

        /// <summary>Метод Рунге-Кутта 4-го порядка (сводка формул лекции).</summary>
        public static void RungeKutta1(double x0, double y0, double h, int n, double[] X, double[] Y)
        {
            double x = x0;
            double y = y0;
            X[0] = x;
            Y[0] = y;
            for (int i = 1; i <= n; i++)
            {
                double k1 = h * Func1(x, y);
                double k2 = h * Func1(x + h / 2, y + k1 / 2);
                double k3 = h * Func1(x + h / 2, y + k2 / 2);
                double k4 = h * Func1(x + h, y + k3);
                double Fi = (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                y = y + Fi;
                x = x + h;
                X[i] = x;
                Y[i] = y;
            }
        }

        // ==================== ОДУ 2-го порядка ====================

        /// <summary>Метод Эйлера для системы y' = z; z' = -z/x - y.</summary>
        public static void Euler2(double x0, double y0, double z0, double h, int n,
                                  double[] X, double[] Y, double[] Z)
        {
            double x = x0;
            double y = y0;
            double z = z0;
            X[0] = x;
            Y[0] = y;
            Z[0] = z;
            for (int i = 1; i <= n; i++)
            {
                double dy = Func2y(x, y, z);
                double dz = Func2z(x, y, z);
                y = y + h * dy;
                z = z + h * dz;
                x = x + h;
                X[i] = x;
                Y[i] = y;
                Z[i] = z;
            }
        }

        /// <summary>Метод Рунге-Кутта для системы y' = z; z' = -z/x - y.</summary>
        public static void RungeKutta2(double x0, double y0, double z0, double h, int n,
                                       double[] X, double[] Y, double[] Z)
        {
            double x = x0;
            double y = y0;
            double z = z0;
            X[0] = x;
            Y[0] = y;
            Z[0] = z;
            for (int i = 1; i <= n; i++)
            {
                double k1y = h * Func2y(x, y, z);
                double k1z = h * Func2z(x, y, z);

                double k2y = h * Func2y(x + h / 2, y + k1y / 2, z + k1z / 2);
                double k2z = h * Func2z(x + h / 2, y + k1y / 2, z + k1z / 2);

                double k3y = h * Func2y(x + h / 2, y + k2y / 2, z + k2z / 2);
                double k3z = h * Func2z(x + h / 2, y + k2y / 2, z + k2z / 2);

                double k4y = h * Func2y(x + h, y + k3y, z + k3z);
                double k4z = h * Func2z(x + h, y + k3y, z + k3z);

                double Fy = (k1y + 2 * k2y + 2 * k3y + k4y) / 6;
                double Fz = (k1z + 2 * k2z + 2 * k3z + k4z) / 6;

                y = y + Fy;
                z = z + Fz;
                x = x + h;
                X[i] = x;
                Y[i] = y;
                Z[i] = z;
            }
        }

        // ==================== система трёх уравнений ====================

        /// <summary>Метод Эйлера для системы трёх уравнений.</summary>
        public static void Euler3(double t0, double x0, double y0, double z0, double h, int n,
                                  double[] T, double[] X, double[] Y, double[] Z)
        {
            double t = t0;
            double x = x0;
            double y = y0;
            double z = z0;
            T[0] = t;
            X[0] = x;
            Y[0] = y;
            Z[0] = z;
            for (int i = 1; i <= n; i++)
            {
                double dx = Func3x(t, x, y, z);
                double dy = Func3y(t, x, y, z);
                double dz = Func3z(t, x, y, z);
                x = x + h * dx;
                y = y + h * dy;
                z = z + h * dz;
                t = t + h;
                T[i] = t;
                X[i] = x;
                Y[i] = y;
                Z[i] = z;
            }
        }

        /// <summary>Метод Рунге-Кутта для системы трёх уравнений.</summary>
        public static void RungeKutta3(double t0, double x0, double y0, double z0, double h, int n,
                                       double[] T, double[] X, double[] Y, double[] Z)
        {
            double t = t0;
            double x = x0;
            double y = y0;
            double z = z0;
            T[0] = t;
            X[0] = x;
            Y[0] = y;
            Z[0] = z;
            for (int i = 1; i <= n; i++)
            {
                double k1x = h * Func3x(t, x, y, z);
                double k1y = h * Func3y(t, x, y, z);
                double k1z = h * Func3z(t, x, y, z);

                double k2x = h * Func3x(t + h / 2, x + k1x / 2, y + k1y / 2, z + k1z / 2);
                double k2y = h * Func3y(t + h / 2, x + k1x / 2, y + k1y / 2, z + k1z / 2);
                double k2z = h * Func3z(t + h / 2, x + k1x / 2, y + k1y / 2, z + k1z / 2);

                double k3x = h * Func3x(t + h / 2, x + k2x / 2, y + k2y / 2, z + k2z / 2);
                double k3y = h * Func3y(t + h / 2, x + k2x / 2, y + k2y / 2, z + k2z / 2);
                double k3z = h * Func3z(t + h / 2, x + k2x / 2, y + k2y / 2, z + k2z / 2);

                double k4x = h * Func3x(t + h, x + k3x, y + k3y, z + k3z);
                double k4y = h * Func3y(t + h, x + k3x, y + k3y, z + k3z);
                double k4z = h * Func3z(t + h, x + k3x, y + k3y, z + k3z);

                double Fx = (k1x + 2 * k2x + 2 * k3x + k4x) / 6;
                double Fy = (k1y + 2 * k2y + 2 * k3y + k4y) / 6;
                double Fz = (k1z + 2 * k2z + 2 * k3z + k4z) / 6;

                x = x + Fx;
                y = y + Fy;
                z = z + Fz;
                t = t + h;
                T[i] = t;
                X[i] = x;
                Y[i] = y;
                Z[i] = z;
            }
        }

        // ==================== общие функции для окна ====================

        /// <summary>Решение задачи выбранным методом.</summary>
        public static OdeSolution Solve(OdeType type, OdeMethod method,
                                        double x0, double xEnd, int n,
                                        double u0, double v0, double w0)
        {
            if (n < 1)
            {
                n = 1;
            }
            double h = (xEnd - x0) / n;
            OdeSolution Res = new OdeSolution();

            if (type == OdeType.FirstOrder)
            {
                Res.X = new double[n + 1];
                Res.U = new double[1][];
                Res.U[0] = new double[n + 1];
                Res.Names = new string[] { "y" };
                if (method == OdeMethod.Euler)
                {
                    Euler1(x0, u0, h, n, Res.X, Res.U[0]);
                }
                else
                {
                    RungeKutta1(x0, u0, h, n, Res.X, Res.U[0]);
                }
            }
            else if (type == OdeType.SecondOrder)
            {
                Res.X = new double[n + 1];
                Res.U = new double[2][];
                Res.U[0] = new double[n + 1];
                Res.U[1] = new double[n + 1];
                Res.Names = new string[] { "y", "y′" };
                if (method == OdeMethod.Euler)
                {
                    Euler2(x0, u0, v0, h, n, Res.X, Res.U[0], Res.U[1]);
                }
                else
                {
                    RungeKutta2(x0, u0, v0, h, n, Res.X, Res.U[0], Res.U[1]);
                }
            }
            else
            {
                Res.X = new double[n + 1];
                Res.U = new double[3][];
                Res.U[0] = new double[n + 1];
                Res.U[1] = new double[n + 1];
                Res.U[2] = new double[n + 1];
                Res.Names = new string[] { "x", "y", "z" };
                if (method == OdeMethod.Euler)
                {
                    Euler3(x0, u0, v0, w0, h, n, Res.X, Res.U[0], Res.U[1], Res.U[2]);
                }
                else
                {
                    RungeKutta3(x0, u0, v0, w0, h, n, Res.X, Res.U[0], Res.U[1], Res.U[2]);
                }
            }

            return Res;
        }

        /// <summary>
        /// Максимальное расхождение двух решений в общих узлах.
        /// Решение B считается с шагом в mult раз меньшим (индексы B: i * mult).
        /// </summary>
        public static double MaxDifference(OdeSolution a, OdeSolution b, int mult)
        {
            double Max = 0;
            for (int j = 0; j < a.U.Length; j++)
            {
                for (int i = 0; i < a.X.Length; i++)
                {
                    double d = Math.Abs(a.U[j][i] - b.U[j][i * mult]);
                    if (d > Max)
                    {
                        Max = d;
                    }
                }
            }
            return Max;
        }

        /// <summary>Название метода.</summary>
        public static string MethodName(OdeMethod m)
        {
            if (m == OdeMethod.Euler)
            {
                return "Метод Эйлера";
            }
            else
            {
                return "Метод Рунге-Кутта";
            }
        }

        /// <summary>Название типа задачи.</summary>
        public static string TypeName(OdeType t)
        {
            if (t == OdeType.FirstOrder)
            {
                return "ОДУ 1-го порядка";
            }
            else if (t == OdeType.SecondOrder)
            {
                return "ОДУ 2-го порядка";
            }
            else
            {
                return "Система трёх уравнений";
            }
        }

        /// <summary>Запись уравнения для окна.</summary>
        public static string Equation(OdeType t)
        {
            if (t == OdeType.FirstOrder)
            {
                return "y′ = y · (1 − x),   y(x₀) = y₀,   x ∈ [x₀; b]";
            }
            else if (t == OdeType.SecondOrder)
            {
                return "y″ + y′/x + y = 0,   y(x₀) = y₀,   y′(x₀) = y′₀";
            }
            else
            {
                return "dx/dt = −2x + 5z\r\ndy/dt = sin(t − 1)·x − y + 3z\r\ndz/dt = −x + 2z";
            }
        }

        /// <summary>Начальные условия по умолчанию (контрольные примеры 1, 2, 3).</summary>
        public static void Defaults(OdeType t, out double x0, out double xEnd, out double h,
                                    out double u0, out double v0, out double w0)
        {
            if (t == OdeType.FirstOrder)
            {
                x0 = 0;
                xEnd = 1;
                h = 0.1;
                u0 = 1;
                v0 = 0;
                w0 = 0;
            }
            else if (t == OdeType.SecondOrder)
            {
                x0 = 1;
                xEnd = 1.5;
                h = 0.1;
                u0 = 0.77;
                v0 = -0.44;
                w0 = 0;
            }
            else
            {
                x0 = 0;
                xEnd = 0.3;
                h = 0.003;
                u0 = 2;
                v0 = 1;
                w0 = 1;
            }
        }
    }
}
