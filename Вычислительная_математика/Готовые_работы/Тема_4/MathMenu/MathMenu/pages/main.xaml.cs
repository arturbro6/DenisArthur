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

namespace MathMenu.pages
{
    /// <summary>
    /// Логика взаимодействия для main.xaml
    /// </summary>
    public partial class main : Page
    {
        public main()
        {
            InitializeComponent();
        }

        private void DigitInteg_Click(object sender, RoutedEventArgs e)
        {
            OdePanel.Visibility = Visibility.Collapsed;

            DigitPanel.Visibility = Visibility.Visible;
            DBLabel.Visibility = Visibility.Collapsed;
            MethodBox.Visibility = Visibility.Collapsed;
            MethodBox.Items.Clear();
            DigitBox.SelectedIndex = 0;
            HintLabel.Content = "выберите способ задания шага и метод вычисления";
        }

        private void DiffUrav_Click(object sender, RoutedEventArgs e)
        {
            DigitPanel.Visibility = Visibility.Collapsed;

            OdePanel.Visibility = Visibility.Visible;
            OdeLabel.Visibility = Visibility.Collapsed;
            OdeMethodBox.Visibility = Visibility.Collapsed;
            OdeMethodBox.SelectedIndex = 0;
            TypeBox.SelectedIndex = 0;
            HintLabel.Content = "выберите тип уравнения и метод вычисления";
        }

        private void DigitBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DBLabel.Visibility = Visibility.Collapsed;
            MethodBox.Visibility = Visibility.Collapsed;
            MethodBox.Items.Clear();

            if (DigitBox.SelectedIndex <= 0)
            {
                return;
            }

            MethodBox.Items.Add(new ComboBoxItem { Content = "— не выбрано —" });

            if (DigitBox.SelectedIndex == 1)
            {
                // постоянный шаг — четыре формулы численного интегрирования
                MethodBox.Items.Add(new ComboBoxItem { Content = "метод левых прямоугольников" });
                MethodBox.Items.Add(new ComboBoxItem { Content = "метод правых прямоугольников" });
                MethodBox.Items.Add(new ComboBoxItem { Content = "метод трапеций" });
                MethodBox.Items.Add(new ComboBoxItem { Content = "метод Симпсона" });
            }
            else
            {
                // переменный шаг — выбор способа управления шагом
                MethodBox.Items.Add(new ComboBoxItem { Content = "метод Рунге (автошаг)" });
                MethodBox.Items.Add(new ComboBoxItem { Content = "адаптивный шаг" });
            }

            MethodBox.SelectedIndex = 0;
            DBLabel.Visibility = Visibility.Visible;
            MethodBox.Visibility = Visibility.Visible;
        }

        private void MethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MethodBox.SelectedIndex <= 0 || DigitBox.SelectedIndex <= 0)
            {
                return;
            }

            int step = DigitBox.SelectedIndex - 1;
            int method = MethodBox.SelectedIndex - 1;

            DBLabel.Visibility = Visibility.Collapsed;
            MethodBox.Visibility = Visibility.Collapsed;
            MethodBox.SelectedIndex = 0;

            NavigationService.Navigate(new pages.DigitInter(step, method));
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OdeLabel.Visibility = Visibility.Collapsed;
            OdeMethodBox.Visibility = Visibility.Collapsed;
            OdeMethodBox.SelectedIndex = 0;

            if (TypeBox.SelectedIndex <= 0)
            {
                return;
            }

            OdeLabel.Visibility = Visibility.Visible;
            OdeMethodBox.Visibility = Visibility.Visible;
        }

        private void OdeMethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OdeMethodBox.SelectedIndex <= 0 || TypeBox.SelectedIndex <= 0)
            {
                return;
            }

            int type = TypeBox.SelectedIndex - 1;
            int method = OdeMethodBox.SelectedIndex - 1;

            OdeLabel.Visibility = Visibility.Collapsed;
            OdeMethodBox.Visibility = Visibility.Collapsed;
            OdeMethodBox.SelectedIndex = 0;

            NavigationService.Navigate(new pages.OdeSolve(type, method));
        }
    }
}
