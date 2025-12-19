using System.Collections.Generic;
using System.IO;

namespace PREACT.Fire
{
    public class FWICalculator
    {
        public static void CalculateYear(Year year, string outputName)
        {
            List<string> output = new List<string>();
            AddHeader(output);
            CalculateOneYear(year, output);
            SaveToFile(outputName, output);
        }

        /*public static void Calculate(YearCollection input, string outputName)
        {
            List<string> output = new List<string>();
            List<double> allFWIValues = new List<double>();
            StatisticalFWI statisticalFWI = new StatisticalFWI();
            AddHeader(output);

            for (int i = 0; i < input.years.Count; i++)
            {
                CalculateOneYear(input.years[i], output, allFWIValues, i == 0, statisticalFWI);
            }
            double mean = MathNet.Numerics.Statistics.Statistics.Mean(allFWIValues);
            double stdDev = MathNet.Numerics.Statistics.Statistics.StandardDeviation(allFWIValues);
            double skewness = MathNet.Numerics.Statistics.Statistics.Skewness(allFWIValues);
            double variance = MathNet.Numerics.Statistics.Statistics.Variance(allFWIValues);
            //MonoBehaviour.print("Mean: " + mean + ", stDev: " + stdDev + ", skewness: " + skewness + ", variance: " + variance);
            SaveToFile(outputName, output);
            SaveSimpleFWI(outputName, input, allFWIValues);
            SaveFWIPercentiles(outputName, statisticalFWI);
        }*/

        static void AddHeader(List<string> output)
        {
            output.Add("DayIndex,Year,Month,Day,Fine Fuel Moisture Code,Duff Moisture Code,Drought Code,Initial Spread Index,Buildup Index,Fire Weather Index");
        }

        /*static void SaveSimpleFWI(string outputName, YearCollection yearCollection, List<double> allFwiValues)
        {
            List<string> output = new List<string>();
            string header = "DayIndex,";
            for (int i = 0; i < yearCollection.years.Count; i++)
            {
                header += "Year " + yearCollection.years[i].year;
                if (i < yearCollection.years.Count - 1)
                {
                    header += ",";
                }
            }
            output.Add(header);

            for (int i = 0; i < 365; i++)
            {
                string dataLine = "";
                for (int j = 0; j < yearCollection.years.Count; j++)
                {
                    if (j == 0)
                    {
                        dataLine += (i + 1) + ",";
                    }
                    dataLine += allFwiValues[j * 365 + i];
                    if (j < yearCollection.years.Count - 1)
                    {
                        dataLine += ",";
                    }
                }
                output.Add(dataLine);
            }

            File.WriteAllLines(Path.Combine(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "_output"), outputName + "_simple_fwi.csv"), output);
        }*/

        /*static void SaveFWIPercentiles(string outputName, StatisticalFWI statisticalFWI)
        {
            List<string> output = new List<string>();
            output.Add("DayIndex,25%-ile,50%-ile,75%-ile,95%-ile,99%-ile");
            for (int i = 0; i < 365; i++)
            {
                double twentyfive = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 25);
                double fifty = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 50);
                double seventyfive = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 75);
                double ninetyfive = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 95);
                double ninetynine = MathNet.Numerics.Statistics.Statistics.Percentile(statisticalFWI.FWIDays[i].FWIValues, 99);
                output.Add(i + "," + twentyfive + "," + fifty + "," + seventyfive + "," + ninetyfive + "," + ninetynine);
            }

            File.WriteAllLines(Path.Combine(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "_output"), outputName + "_fwi_percentiles.csv"), output);
        }*/

        static void SaveToFile(string outputFilePath, List<string> output)
        {
            File.WriteAllLines( outputFilePath, output);
        }

        static void CalculateOneYear(Year inputYear, List<string> output)
        {
            double temp, rhum, wind, prcp;
            double ffmc, ffmc0, dmc, dmc0, dc, dc0, isi, bui, fwi, ffwi, G, aPRCP;
            int year, month, day;
            // Initialize FMC, DMC, and DC
            ffmc0 = 85.0;
            dmc0 = 6.0;
            dc0 = 15.0;

            int dayIndex = 0;
            for (int j = 0; j < inputYear.months.Length; j++)
            {
                year = inputYear.year;
                for (int k = 0; k < inputYear.months[j].days.Length; k++)
                {
                    month = j + 1;
                    day = k + 1;
                    Day dayData = inputYear.months[j].days[k];
                    temp = dayData.tempMax;
                    rhum = dayData.rhumMin;
                    wind = dayData.windMax;
                    prcp = dayData.prcpMin;
                    FFMCcalc(temp, rhum, wind, prcp, ffmc0, out ffmc);
                    DMCcalc(temp, rhum, prcp, dmc0, month, out dmc);
                    DCcalc(temp, prcp, dc0, month, out dc);
                    ISIcalc(ffmc, wind, out isi);
                    BUIcalc(dmc, dc, out bui);
                    FWIcalc(isi, bui, out fwi);
                    ffmc0 = ffmc;
                    dmc0 = dmc;
                    dc0 = dc;

                    //do not count november - february
                    if (month < 2 || month > 9)
                    {
                        fwi = 0.0;
                    }

                    if (double.IsNaN(fwi))
                    {
                        fwi = 0.0;
                    }

                    output.Add(dayIndex + ", " + year + "," + month + "," + day + "," + ffmc + "," + dmc + "," + dc + "," + isi + "," + bui + "," + fwi);
                    ++dayIndex;
                }
            }
        }

        static void CalcKBDI(double temp, double annualPRCP, ref double kbdi)
        {
            double fTemp = CtoF(temp);
            double dQ = 0.001 * (800.0 - kbdi) * (0.968 * exp(0.0486 * fTemp) - 0.830) / (1.0 + 10.88 * exp(-0.0441 * mmToInches(annualPRCP)));
            kbdi += System.Math.Max(0, dQ);
        }

        static double mmToInches(double annualPRCP)
        {
            return annualPRCP * 0.03937007874;
        }

        static void CalcNI(double temp, double rhum, double prcp, out double G)
        {
            G = 0.0;
            if (temp > 0 && prcp < 3.0)
            {
                G += temp * (temp - CalcDewPoint(temp, rhum));
            }
        }

        static double CalcDewPoint(double temp, double rhum)
        {
            return temp - (100.0 - rhum) * 0.2;
        }

        static double CtoF(double cTemp)
        {
            return cTemp * 9.0 / 5.0 + 32;
        }

        static double MeterPerSecondToMPH(double v)
        {
            return v * 2.2369356;
        }

        static void FFWIcalc(double temp, double rhum, double wind, out double ffwi)
        {
            double m, eta, fTemp, mphWind;

            fTemp = CtoF(temp);
            mphWind = MeterPerSecondToMPH(wind);

            if (rhum < 10.0)
            {
                m = 0.03229 + 0.281073 * rhum - 0.000578 * rhum * fTemp;
            }
            else if (rhum <= 50.0)
            {
                m = 2.22749 + 0.160107 * rhum - 0.01478 * fTemp;
            }
            else
            {
                m = 21.0606 + 0.005565 * rhum * rhum - 0.00035 * rhum * fTemp - 0.483199 * rhum;
            }
            m /= 30.0;
            eta = 1.0 - 2.0 * m + 1.5 * m * m - 0.5 * m * m * m;

            ffwi = eta * sqrt(1.0 + mphWind * mphWind) / 0.3002;
        }

        static void FFMCcalc(double T, double H, double W, double Ro, double Fo, out double ffmc)
        {
            double Mo, Rf, Ed, Ew, M, Kl, Kw, Mr, Ko, Kd;
            Mo = 147.2 * (101.0 - Fo) / (59.5 + Fo); //Eq. 1 in van Wagner and Pickett (1985)
            if (Ro > 0.5)
            {
                Rf = Ro - 0.5; //Eq.2
                if (Mo <= 150.0)
                {
                    Mr = Mo + 42.5 * Rf * (exp(-100.0 / (251.0 - Mo))) * (1 - exp(-6.93 / Rf)); //Eq. 3a
                }
                else
                {
                    Mr = Mo + 42.5 * Rf * (exp(-100.0 / (251.0 - Mo))) * (1 - exp(-6.93 / Rf)) + .0015 * pow(Mo - 150.0, 2.0) * pow(Rf, 0.5); //Eq. 3b
                }
                if (Mr > 250.0)
                {
                    Mr = 250.0;
                }
                Mo = Mr;
            }
            Ed = 0.942 * pow(H, 0.679) + 11.0 * exp((H - 100.0) / 10.0) + 0.18 * (21.1 - T) * (1.0 - exp(-0.115 * H)); //Eq. 4
            if (Mo > Ed)
            {
                Ko = 0.424 * (1.0 - pow(H / 100.0, 1.7)) + 0.0694 * pow(W, .5) * (1.0 - pow(H / 100.0, 8.0)); //Eq. 6a
                Kd = Ko * 0.581 * exp(0.0365 * T); //Eq. 6b
                M = Ed + (Mo - Ed) * pow(10.0, -Kd); //Eq. 8
            }
            else
            {
                Ew = 0.618 * pow(H, .753) + 10.0 * exp((H - 100.0) / 10.0) + 0.18 * (21.1 - T) * (1.0 - exp(-0.115 * H)); //Eq. 5
                if (Mo < Ew)
                {
                    Kl = 0.424 * (1.0 - pow((100.0 - H) / 100.0, 1.7)) + 0.0694 * pow(W, .5) * (1 - pow((100.0 - H) / 100.0, 8.0)); //Eq. 7a
                    Kw = Kl * .581 * exp(0.0365 * T); //Eq. 7b
                    M = Ew - (Ew - Mo) * pow(10.0, -Kw); //Eq. 9
                }
                else
                {
                    M = Mo;
                }
            }
            //Finally calculate FFMC 
            ffmc = (59.5 * (250.0 - M)) / (147.2 + M);
            //..............................
            //Make sure 0. <= FFMC <= 101.0 
            //..............................
            if (ffmc > 101.0) ffmc = 101.0;
            if (ffmc <= 0.0) ffmc = 0.0;
        }

        // DMC calculation 
        static void DMCcalc(double T, double H, double Ro, double Po, int I, out double dmc)
        {
            double Re, Mo, Mr, K, B, P, Pr;
            double[] Le = { 6.5, 7.5, 9.0, 12.8, 13.9, 13.9, 12.4, 10.9, 9.4, 8.0, 7.0, 6.0 };
            if (T >= -1.1)
            {
                K = 1.894 * (T + 1.1) * (100.0 - H) * Le[I - 1] * 0.0001; //Eq. 16
            }
            else
            {
                K = 0.0; //Eq. 17
            }

            if (Ro <= 1.5)
            {
                Pr = Po;
            }
            else
            {
                Re = 0.92 * Ro - 1.27; //Eq. 11
                Mo = 20.0 + 280.0 / exp(0.023 * Po); //Eq. 12
                if (Po <= 33.0)
                {
                    B = 100.0 / (0.5 + 0.3 * Po); //Eq. 13a
                }
                else
                {
                    if (Po <= 65.0)
                    {
                        B = 14.0 - 1.3 * log(Po); //Eq. 13b
                    }
                    else
                    {
                        B = 6.2 * log(Po) - 17.2; //Eq. 13c
                    }
                }
                Mr = Mo + 1000.0 * Re / (48.77 + B * Re); //Eq. 14
                Pr = 43.43 * (5.6348 - log(Mr - 20.0)); //Eq. 15
            }
            if (Pr < 0.0)
            {
                Pr = 0.0;
            }
            P = Pr + K;
            if (P <= 0.0)
            {
                P = 0.0;
            }
            dmc = P;
        }

        // DC calculation 
        static void DCcalc(double T, double Ro, double Do, int I, out double dc)
        {
            double Rd, Qo, Qr, V, Dr;
            double[] Lf = { -1.6, -1.6, -1.6, 0.9, 3.8, 5.8, 6.4, 5.0, 2.4, 0.4, -1.6, -1.6 };
            if (Ro > 2.8)
            {
                Rd = 0.83 * (Ro) - 1.27; //Eq. 18
                Qo = 800.0 * exp(-Do / 400.0); //Eq. 19
                Qr = Qo + 3.937 * Rd; //Eq. 20
                Dr = 400.0 * log(800.0 / Qr); //Eq. 21
                if (Dr > 0.0)
                {
                    Do = Dr;
                }
                else
                {
                    Do = 0.0;
                }
            }
            //Eq. 22
            if (T > -2.8)
            {
                V = 0.36 * (T + 2.8) + Lf[I - 1];
            }
            else
            {
                V = Lf[I - 1];
            }
            if (V < 0.0)
            {
                V = 0.0;
            }
            dc = Do + 0.5 * V; //Eq. 23
        }

        // ISI calculation 
        static void ISIcalc(double F, double W, out double isi)
        {
            double Fw, M, Ff;
            M = 147.2 * (101 - F) / (59.5 + F); //Eq. 1
            Fw = exp(0.05039 * W); //Eq. 24
            Ff = 91.9 * exp(-.1386 * M) * (1.0 + pow(M, 5.31) / 4.93E7); //Eq. 25
            isi = 0.208 * Fw * Ff; //Eq. 26
        }

        // BUI calculation 
        static void BUIcalc(double P, double D, out double bui)
        {
            if (P <= 0.4 * D)
            {
                bui = 0.8 * P * D / (P + .4 * D); //Eq. 27a
            }
            else
            {
                bui = P - (1.0 - .8 * D / (P + 0.4 * D)) * (0.92 + pow(.0114 * P, 1.7)); //Eq. 27b
            }
            if (bui <= 0.0)
            {
                bui = 0.0;
            }
        }

        // FWI calculation 
        static void FWIcalc(double R, double U, out double fwi)
        {
            double Fd, B;
            if (U <= 80.0)
            {
                Fd = 0.626 * pow(U, 0.809) + 2.0; //Eq. 28a
            }
            else
            {
                Fd = 1000.0 / (25.0 + 108.64 * exp(-0.023 * U)); //Eq. 28b
            }
            B = 0.1 * R * Fd;  //Eq. 29
            if (B > 1.0)
            {
                fwi = exp(2.72 * pow(0.434 * log(B), 0.647)); //Eq. 30a
            }
            else
            {
                fwi = B; //Eq. 30b
            }
        }

        static double exp(double x)
        {
            return System.Math.Exp(x);
        }

        static double pow(double x, double y)
        {
            return System.Math.Pow(x, y);
        }

        static double log(double x)
        {
            return System.Math.Log(x);
        }

        static double sqrt(double x)
        {
            return System.Math.Sqrt(x);
        }
    }

}
