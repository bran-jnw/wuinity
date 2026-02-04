using System;
using static System.Math;

namespace PREACT.Wildfire
{
    public class FireWeatherIndex
    {
        double _ffmc, _ffmc0, _dmc, _dmc0, _dc, _dc0, _isi, _bui, _fwi, _ffwi; //, _G, _aPRCP

        public double FFMC { get => _ffmc; }
        public double DMC { get => _dmc; }
        public double DC { get => _dc; }
        public double ISI { get => _isi; }
        public double BUI { get => _bui; }
        public double FWI { get => _fwi; }

        public FireWeatherIndex() 
        {
            Reset();
        }

        public void Reset()
        {
            // Initialize FMC, DMC, and DC
            _ffmc0 = 85.0;
            _dmc0 = 6.0;
            _dc0 = 15.0;
        }

        public void CalculateDay(DateTime dateTime, double temp, double rhum, double wind, double prcp)
        {
            FFMCcalc(temp, rhum, wind, prcp, _ffmc0, out _ffmc);
            DMCcalc(temp, rhum, prcp, _dmc0, dateTime.Month, out _dmc);
            DCcalc(temp, prcp, _dc0, dateTime.Month, out _dc);
            ISIcalc(_ffmc, wind, out _isi);
            BUIcalc(_dmc, _dc, out _bui);
            FWIcalc(_isi, _bui, out _fwi);
            _ffmc0 = _ffmc;
            _dmc0 = _dmc;
            _dc0 = _dc;

            //do not count november - february
            if (dateTime.Month < 2 || dateTime.Month > 9)
            {
                _fwi = 0.0;
            }

            if (double.IsNaN(_fwi))
            {
                _fwi = 0.0;
            }
        }

        public static void CalcKBDI(double temp, double annualPRCP, ref double kbdi)
        {
            double fTemp = CtoF(temp);
            double dQ = 0.001 * (800.0 - kbdi) * (0.968 * Exp(0.0486 * fTemp) - 0.830) / (1.0 + 10.88 * Exp(-0.0441 * mmToInches(annualPRCP)));
            kbdi += System.Math.Max(0, dQ);
        }

        public static double mmToInches(double annualPRCP)
        {
            return annualPRCP * 0.03937007874;
        }

        public static void CalcNI(double temp, double rhum, double prcp, out double G)
        {
            G = 0.0;
            if (temp > 0 && prcp < 3.0)
            {
                G += temp * (temp - CalcDewPoint(temp, rhum));
            }
        }

        public static double CalcDewPoint(double temp, double rhum)
        {
            return temp - (100.0 - rhum) * 0.2;
        }

        public static double CtoF(double cTemp)
        {
            return cTemp * 9.0 / 5.0 + 32;
        }

        static double MeterPerSecondToMPH(double v)
        {
            return v * 2.2369356;
        }

        public static void FFWIcalc(double temp, double rhum, double wind, out double ffwi)
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

            ffwi = eta * Sqrt(1.0 + mphWind * mphWind) / 0.3002;
        }

        public static void FFMCcalc(double T, double H, double W, double Ro, double Fo, out double ffmc)
        {
            double Mo, Rf, Ed, Ew, M, Kl, Kw, Mr, Ko, Kd;
            Mo = 147.2 * (101.0 - Fo) / (59.5 + Fo); //Eq. 1 in van Wagner and Pickett (1985)
            if (Ro > 0.5)
            {
                Rf = Ro - 0.5; //Eq.2
                if (Mo <= 150.0)
                {
                    Mr = Mo + 42.5 * Rf * (Exp(-100.0 / (251.0 - Mo))) * (1 - Exp(-6.93 / Rf)); //Eq. 3a
                }
                else
                {
                    Mr = Mo + 42.5 * Rf * (Exp(-100.0 / (251.0 - Mo))) * (1 - Exp(-6.93 / Rf)) + .0015 * Pow(Mo - 150.0, 2.0) * Pow(Rf, 0.5); //Eq. 3b
                }
                if (Mr > 250.0)
                {
                    Mr = 250.0;
                }
                Mo = Mr;
            }
            Ed = 0.942 * Pow(H, 0.679) + 11.0 * Exp((H - 100.0) / 10.0) + 0.18 * (21.1 - T) * (1.0 - Exp(-0.115 * H)); //Eq. 4
            if (Mo > Ed)
            {
                Ko = 0.424 * (1.0 - Pow(H / 100.0, 1.7)) + 0.0694 * Pow(W, .5) * (1.0 - Pow(H / 100.0, 8.0)); //Eq. 6a
                Kd = Ko * 0.581 * Exp(0.0365 * T); //Eq. 6b
                M = Ed + (Mo - Ed) * Pow(10.0, -Kd); //Eq. 8
            }
            else
            {
                Ew = 0.618 * Pow(H, .753) + 10.0 * Exp((H - 100.0) / 10.0) + 0.18 * (21.1 - T) * (1.0 - Exp(-0.115 * H)); //Eq. 5
                if (Mo < Ew)
                {
                    Kl = 0.424 * (1.0 - Pow((100.0 - H) / 100.0, 1.7)) + 0.0694 * Pow(W, .5) * (1 - Pow((100.0 - H) / 100.0, 8.0)); //Eq. 7a
                    Kw = Kl * .581 * Exp(0.0365 * T); //Eq. 7b
                    M = Ew - (Ew - Mo) * Pow(10.0, -Kw); //Eq. 9
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
        public static void DMCcalc(double T, double H, double Ro, double Po, int I, out double dmc)
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
                Mo = 20.0 + 280.0 / Exp(0.023 * Po); //Eq. 12
                if (Po <= 33.0)
                {
                    B = 100.0 / (0.5 + 0.3 * Po); //Eq. 13a
                }
                else
                {
                    if (Po <= 65.0)
                    {
                        B = 14.0 - 1.3 * Log(Po); //Eq. 13b
                    }
                    else
                    {
                        B = 6.2 * Log(Po) - 17.2; //Eq. 13c
                    }
                }
                Mr = Mo + 1000.0 * Re / (48.77 + B * Re); //Eq. 14
                Pr = 43.43 * (5.6348 - Log(Mr - 20.0)); //Eq. 15
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
        public static void DCcalc(double T, double Ro, double Do, int I, out double dc)
        {
            double Rd, Qo, Qr, V, Dr;
            double[] Lf = { -1.6, -1.6, -1.6, 0.9, 3.8, 5.8, 6.4, 5.0, 2.4, 0.4, -1.6, -1.6 };
            if (Ro > 2.8)
            {
                Rd = 0.83 * (Ro) - 1.27; //Eq. 18
                Qo = 800.0 * Exp(-Do / 400.0); //Eq. 19
                Qr = Qo + 3.937 * Rd; //Eq. 20
                Dr = 400.0 * Log(800.0 / Qr); //Eq. 21
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
        public static void ISIcalc(double F, double W, out double isi)
        {
            double Fw, M, Ff;
            M = 147.2 * (101 - F) / (59.5 + F); //Eq. 1
            Fw = Exp(0.05039 * W); //Eq. 24
            Ff = 91.9 * Exp(-.1386 * M) * (1.0 + Pow(M, 5.31) / 4.93E7); //Eq. 25
            isi = 0.208 * Fw * Ff; //Eq. 26
        }

        // BUI calculation 
        public static void BUIcalc(double P, double D, out double bui)
        {
            if (P <= 0.4 * D)
            {
                bui = 0.8 * P * D / (P + .4 * D); //Eq. 27a
            }
            else
            {
                bui = P - (1.0 - .8 * D / (P + 0.4 * D)) * (0.92 + Pow(.0114 * P, 1.7)); //Eq. 27b
            }
            if (bui <= 0.0)
            {
                bui = 0.0;
            }
        }

        // FWI calculation 
        public static void FWIcalc(double R, double U, out double fwi)
        {
            double Fd, B;
            if (U <= 80.0)
            {
                Fd = 0.626 * Pow(U, 0.809) + 2.0; //Eq. 28a
            }
            else
            {
                Fd = 1000.0 / (25.0 + 108.64 * Exp(-0.023 * U)); //Eq. 28b
            }
            B = 0.1 * R * Fd;  //Eq. 29
            if (B > 1.0)
            {
                fwi = Exp(2.72 * Pow(0.434 * Log(B), 0.647)); //Eq. 30a
            }
            else
            {
                fwi = B; //Eq. 30b
            }
        }
    }
}
