using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using MathNet.Numerics.Integration;
using LiveCharts.Defaults;
using LiveCharts;

namespace CoordinateTransformations
{
    public partial class MainWindow : Window
    {
        GPSData gpsf = new GPSData();
        Calculation calc = new Calculation();
        CoordinatesOfPoints cop = new CoordinatesOfPoints();
        List<CoordinatesOfPoints> c = new List<CoordinatesOfPoints>();
        List<CoordinatesOfPoints> export_list = new List<CoordinatesOfPoints>();

        bool PossibleExportToFile = false;
        bool PossibleCalculation = false;
        string filename;

        public MainWindow()
        {
            InitializeComponent();

            aTB.Text = calc.a.ToString();
            bTB.Text = calc.b.ToString();
            m0TB.Text = calc.m0.ToString();
            L0CB.SelectedIndex = 1;

            PossibleCalculation = false;

        }

        public  ChartValues<ObservablePoint> ValuesA { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "txt file|*.txt";
            dlg.Title = "Select file with GPS frame";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                filename = dlg.FileName;
                var readText = File.ReadAllLines(filename);
                for (int i = 0; i < readText.Count(); i++)
                {
                    calc.SeparateInfoFromGPSData(readText[i]);
                    c.Add(new CoordinatesOfPoints() { XPoints = calc.calculate_xValue(), YPoints = calc.calculate_yValue() });
                    export_list.Add(new CoordinatesOfPoints() { XPoints = calc.calculate_xValue(), YPoints = calc.calculate_yValue(), hValues = calc.calculate_hValue(), HValues = calc.calculate_HValue() });
                }

                ValuesA = new ChartValues<ObservablePoint>();

                foreach (CoordinatesOfPoints cops in c)
                {
                    ValuesA.Add(new ObservablePoint(cops.YPoints, cops.XPoints));
                }
                DataContext = this;
                PossibleCalculation = true;
                PossibleExportToFile = true;

            }
            PossibleCalculation = false;
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (PossibleExportToFile == true)
            {
                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "(*.txt)|*.txt";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                StringBuilder sb = new StringBuilder();

                foreach (CoordinatesOfPoints cops in export_list)
                {
                    double xx = cops.XPoints;
                    double yy = cops.YPoints;
                    double hh = cops.hValues;
                    double HH = cops.HValues;
                    sb.Append(xx);
                    sb.Append(";");
                    sb.Append(yy);
                    sb.Append(";");
                    sb.Append(hh);
                    sb.Append(";");
                    sb.Append(HH);
                    sb.AppendLine();
                }


                var save_txt = "nazwa";
                if (saveFileDialog1.ShowDialog() == true)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        save_txt = saveFileDialog1.FileName;
                        myStream.Close();
                        File.WriteAllText(save_txt, sb.ToString(), Encoding.UTF8);
                        var msg = "Zapisano pomyślnie : " + save_txt;
                        MessageBox.Show(msg, "Zapis");
                    }
                }
            }
        }

        private void aTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            PossibleCalculation = true;
            calc.a = Convert.ToDouble(aTB.Text);
        }

        private void bTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            PossibleCalculation = true;
            calc.b = Convert.ToDouble(bTB.Text);

        }

        private void m0TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            PossibleCalculation = true;
            calc.m0 = Convert.ToDouble(m0TB.Text);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PossibleCalculation = true;
        }

        private void ButtonRecalculate_Click(object sender, RoutedEventArgs e)
        {
            if (PossibleCalculation)
            {
                var readText = File.ReadAllLines(filename);
                c.Clear();
                export_list.Clear();
                calc.l0 = int.Parse(L0CB.Text);
                for (int i = 0; i < readText.Count(); i++)
                {
                    calc.SeparateInfoFromGPSData(readText[i]);
                    c.Add(new CoordinatesOfPoints()
                    {
                        XPoints = calc.calculate_xValue(),
                        YPoints = calc.calculate_yValue()
                    });
                    export_list.Add(new CoordinatesOfPoints()
                    {
                        XPoints = calc.calculate_xValue(),
                        YPoints = calc.calculate_yValue(),
                        hValues = calc.calculate_hValue(),
                        HValues = calc.calculate_HValue()
                    });

                }

                if (c.Any(point => HasInvalidValue(point))) return;

                ValuesA.Clear();

                foreach (CoordinatesOfPoints cops in c)
                {
                    ValuesA.Add(new ObservablePoint(cops.YPoints, cops.XPoints));
                }
                
                PossibleExportToFile = true;
            }
        }

        private static bool HasInvalidValue(CoordinatesOfPoints c)
        {
            return double.IsNaN(c.XPoints) || double.IsInfinity(c.XPoints) || double.IsNaN(c.YPoints) || double.IsInfinity(c.YPoints);
        }

        private void CartesianChart_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
