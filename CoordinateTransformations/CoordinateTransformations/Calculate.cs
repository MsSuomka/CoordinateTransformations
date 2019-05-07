using MathNet.Numerics.Integration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateTransformations
{
    public class Calculation
    {
        GPSData gpsData = new GPSData();
        public double a = 6378137.0;
        public double b = 6356752.314;
        public int l0 = 18;
        public double m0 = 0.999923;

      
        public void SeparateInfoFromGPSData(String data)
        {
            String[] line = data.Split(',');
            gpsData.UTCtime = double.Parse(line[1], CultureInfo.InvariantCulture);
            gpsData.Latitude = double.Parse(line[2], CultureInfo.InvariantCulture);
            gpsData.Longitude = double.Parse(line[4], CultureInfo.InvariantCulture);
            gpsData.Altitude = double.Parse(line[9], CultureInfo.InvariantCulture);
            gpsData.HeightOfGeoid = double.Parse(line[11], CultureInfo.InvariantCulture);
        }

        public double calculate_LValue()
        {
            double result = (Math.Floor(gpsData.Longitude / 100.0) + (gpsData.Longitude - 100.0 * Math.Floor(gpsData.Longitude / 100.0)) / 60.0)
                            * Math.PI / 180.0;
            return result;
        }

        public double calculate_BValue()
        {
            double result = (Math.Floor(gpsData.Latitude / 100.0) + (gpsData.Latitude - 100.0 * Math.Floor(gpsData.Latitude / 100.0)) / 60.0)
                            * Math.PI / 180.0;
            return result;
        }

        public double calculate_hValue()
        {
            double result = gpsData.Altitude;
            return result;
        }

        public double calculate_HValue()
        {
            double result = gpsData.Altitude + gpsData.HeightOfGeoid;
            return result;
        }

        public double calculate_dLValue()
        {
            double L = this.calculate_LValue();
            double L0_rad = l0 * Math.PI / 180;
            double result = L - L0_rad;
            return result;
        }

        public double calculate_Eccentricity()
        {
            double result = Math.Sqrt((Math.Pow(a, 2) - Math.Pow(b, 2)) / Math.Pow(a, 2));
            return result;
        }

        public double calculate_NValue()
        {
            double E = this.calculate_Eccentricity();
            double B = this.calculate_BValue();
            double result = a / (Math.Sqrt(1 - Math.Pow(E, 2) * Math.Pow(Math.Sin(B), 2)));
            return result;
        }

        public double calculate_sValue()
        {
            double E = this.calculate_Eccentricity();
            double B = this.calculate_BValue();

            double Ag = 1.0 + 3.0 / 4.0 * E * E + 45.0 / 64.0 * Math.Pow(E, 4.0) + 175.0 / 256.0 * Math.Pow(E, 6.0)
                        + 11025.0 / 16384.0 * Math.Pow(E, 8.0) + 43659.0 / 65536.0 * Math.Pow(E, 10.0);

            double Bg = 3.0 / 4.0 * E * E + 15.0 / 16.0 * Math.Pow(E, 4.0) + 525.0 / 512.0 * Math.Pow(E, 6.0)
                        + 2205.0 / 2048.0 * Math.Pow(E, 8.0) + 72765.0 / 65536.0 * Math.Pow(E, 10.0);

            double Cg = 15.0 / 64.0 * Math.Pow(E, 4.0) + 105.0 / 256.0 * Math.Pow(E, 6.0) + 2205.0 / 4096.0 * Math.Pow(E, 8.0)
                        + 10395.0 / 16384.0 * Math.Pow(E, 10.0);

            double Dg = 35.0 / 512.0 * Math.Pow(E, 6.0) + 315.0 / 2048.0 * Math.Pow(E, 8.0) + 31185.0 / 131072.0 * Math.Pow(E, 10.0);

            double Eg = 315.0 / 16384.0 * Math.Pow(E, 8.0) + 3465.0 / 65536.0 * Math.Pow(E, 10.0);

            double result = a * (1.0 - E * E) * (Ag * B - (Bg / 2.0) * Math.Sin(2.0 * B) + Cg / 4.0 * Math.Sin(4.0 * B) - Dg / 6.0
                            * Math.Sin(6.0 * B) + Eg / 8.0 * Math.Sin(8.0 * B));

            return result;
        }

        public double calculate_SValue()
        {
            double E = this.calculate_Eccentricity();
            double B = this.calculate_BValue();
            double result = SimpsonRule.IntegrateThreePoint(x => (a * (1.0 - E * E)) / Math.Sqrt((1.0 - E * E * Math.Sin(B) * Math.Sin(B))), 0.0, B);
            return result;
        }

        public double calculate_etaValue()
        {
            double E = this.calculate_Eccentricity();
            double B = this.calculate_BValue();
            double result = Math.Sqrt((Math.Pow(E, 2) * Math.Pow(Math.Cos(B), 2)) / (1 - Math.Pow(E, 2)));
            return result;
        }

        public double calculate_tValue()
        {
            double B = this.calculate_BValue();
            return Math.Tan(B);
        }

        public double calculate_xValue()
        {
            double s = this.calculate_sValue();
            double dL = this.calculate_dLValue();
            double N = this.calculate_NValue();
            double B = this.calculate_BValue();
            double t = this.calculate_tValue();
            double eta = this.calculate_etaValue();

            double result = m0 * (s + dL * dL / 2.0 * N * Math.Sin(B) * Math.Cos(B) + Math.Pow(dL, 4.0) / 24.0 * N * Math.Sin(B)
                            * Math.Pow(Math.Cos(B), 3.0) * (5.0 - t * t + 9.0 * eta * eta + 4.0 * Math.Pow(eta, 4.0)) + Math.Pow(dL, 6.0)
                            / 720.0 * N * Math.Sin(B) * Math.Pow(Math.Cos(B), 5.0) * (61.0 - 58.0 * t * t + Math.Pow(t, 4.0)));
            return result;
        }

        public double calculate_yValue()
        {
            double s = this.calculate_sValue();
            double dL = this.calculate_dLValue();
            double N = this.calculate_NValue();
            double B = this.calculate_BValue();
            double t = this.calculate_tValue();
            double eta = this.calculate_etaValue();

            double result = m0 * (dL * N * Math.Cos(B) + (Math.Pow(dL, 3.0) / 6.0) * N * Math.Pow(Math.Cos(B), 3.0) * (1.0 - Math.Pow(t, 2.0)
                            + Math.Pow(eta, 2.0)) + (Math.Pow(dL, 5.0) / 120.0) * N * Math.Pow(Math.Cos(B), 5.0) * (5.0 - 18.0 * Math.Pow(t, 2)
                            + Math.Pow(t, 4.0) + 14.0 * Math.Pow(eta, 2.0) - 58.0 * eta * eta * t * t)) + 500000.0 + (l0 / 3.0) * 1000000.0;
            return result;
        }
    }
}
