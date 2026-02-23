//https://github.com/emxsys/behave
/* 
 * The MIT License
 *
 * Copyright 2018 Bruce Schubert (original), 2025 Jonathan Wahlqvist (JavaScript to C#)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using static System.Math;
using PREACT.Wildfire.Behave;

namespace PREACT.Wildfire
{   
    /**
     * The Rothermel, et al, fire spread model developed for modeling wildland fire behavior.
     *
     * References:
     * <ul>
     * <li><a name="bib_1000"></a>Albini, F.A., 1976, Estimating Wildfire Behavior and Effects, General
     * Technical Report INT-30, USDA Forest Service, Intermountain Forest and Range Experiment Station
     *
     * <li><a name="bib_1001"></a>Anderson, H.A., 1983, Predicting Wind-driven Wild Land Fire Size and
     * Shape, Research Paper INT-305, USDA Forest Service, Intermountain Forest and Range Experiment
     * Station
     *
     * <li><a name="bib_1002"></a>Anderson, K., 2009, A Comparison of Hourly Fire Fuel Moisture Code
     * Calculations within Canada, Canadian Forest Service
     *
     * <li><a name="bib_1003"></a>Rothermel, R.C., 1972, A mathematical model for predicting fire spread
     * in wildland fuels, General Technical Report INT-115, USDA Forest Service, Intermountain Forest
     * and Range Experiment Station
     *
     * <li><a name="bib_1004"></a>Rothermel et al, 1986, Modeling Moisture Content of Fine Dead Wildland
     * Fuels: Input to the BEHAVE Fire Prediction System, Research Paper, INT-359, USDA Forest Service,
     * Intermountain Research Station
     *
     * <li><a name="bib_1005"></a>Van Wagner, C.E., 1977, A Method of Computing Fine Fuel Moisture
     * Content Throughout the Diurnal Cycle, Information Report PS-X-69, Petawawa Forest Experiment
     * Station, Canadian Forest Service
     * </ul>
     * Other Sources:
     * <ul>
     * <li>BehavePlus5, xfblib.cpp, Copyright Collin D. Bevins.
     * <li>Firelib v1.04, firelib.c, Copyright Collin D. Bevins.
     * </ul>
     *
     * @author Bruce Schubert
     */
    public class Rothermel
    {
        const double HALF_PI = PI / 2;
        const double TO_RADIANS = PI / 180;
        const double TO_DEGREES = 180 / PI;

        public Rothermel(int fuelModelNumber, FuelModelSet fuelModelSet)
        {
            //TOD
        }

        /**
         * Calculates the mean bulk density (fuel-bed weight per unit volume): rho_b.
         * 
         * @param {Number[]} w0 An array of fuel particle loading values [lb/ft2]
         * @param {Number} height The fuel bed height [ft]
         * @return {Number} rho_b [lbs/ft3]
         */
        static double meanBulkDensity(double[] w0, double height)
        {
            if (height <= 0)
            {
                //throw new RangeError("height must be > 0.")
            }

            double w0_t = 0;
            foreach (double d in w0)
            {
                w0_t += d;
            }
            double rho_b = w0_t / height;

            return rho_b;
        }

        /**
         * Calculates the mean packing ratio for the fuel: beta.
         *
         * The compactness of the fuel bed is defined by the packing ratio, which is defined as the
         * fraction of the fuel array volume that is occupied by the fuel. Rothermel 1972: eq. (74)
         *
         * @param {Number} rho_b The mean bulk density of the fuel bed [lbs/ft3].
         * @param {Number} rho_p The oven-dry fuel-particle density [lbs/ft3].
         * @return {Number} beta [dimensionless]
         */
        static double meanPackingRatio(double rho_b, double rho_p)
        {
            if (rho_p <= 0)
            {
                //throw new RangeError("rho_p must be > 0.");
            }

            double beta = rho_b / rho_p;
            if (beta > 0.12 || beta < 0)
            {
                //throw new RangeError("Mean packing ratio [beta] out of limits [0,0.12]: " + beta);
            }

            return beta;
        }

        /**
         * Computes the optimal packing ratio for the fuel: beta_opt.
         *
         * Optimum packing ratio is a term used in the Rothermel's (1972) surface fire spread model
         * indicating the packing ratio that optimizes the reaction velocity term of the spread model.
         * Optimum packing ratio is a function of the fineness of fuel particles, which is measured by
         * the characteristic surface-area-to-volume ratio of the fuelbed. Optimum packing ratio does
         * not optimize fire behavior (rate of spread or fireline intensity). Fire Science Glossary
         * [electronic]. http://www.firewords.net
         *
         * @param {Number} sigma The characteristic SAV ratio for the fuel complex [ft2/ft3].
         * @return {Number} beta_opt [dimensionless]
         */
        static double optimalPackingRatio(double sigma)
        {
            double beta_opt = 3.348 * Pow(sigma, -0.8189);
            return beta_opt;
        }

        /**
         * Computes the characteristic surface-area-to-volume ratio for the fuel complex: sigma.
         *
         * In Rothermel's (1972) surface fire spread model, characteristic surface-area-to-volume (SAV)
         * ratio constitutes the fuelbed-average SAV weighted by particle surface area. Surface-area
         * weighting emphasizes fine fuel because finer fuel particles have larger SAV ratios. Fire
         * Science Glossary [electronic]. http://www.firewords.net
         *
         * Rothermel 1972: eq. (71) and (72).
         *
         * @param {Number[]} sv An array of fuel particle SAV ratio values [ft2/ft3].
         * @param {Number[]} w0 An array of fuel particle loading values [lbs/ft2].
         * @return {Number} sigma [ft2/ft3]
         */
        static double characteristicSAV(double[] sv, double[] w0)
        {
            double sw_t = 0;      // sw = (sv * w)
            double s2w_t = 0;     // s2w = (sv^2 * w)
            int len = sv.Length;
            for (int i = 0; i < len; i++)
            {
                sw_t += sv[i] * w0[i];
                s2w_t += sv[i] * sv[i] * w0[i];
            }
            if (sw_t <= 0)
            {
                //throw new RangeError("w0 total loading must be > 0.");
            }
            double sigma = s2w_t / sw_t;

            return sigma;
        }

        /**
         * Calculates the potential reaction velocity: gamma.
         *
         * Rothermel 1972: eq. (68),(70) and Albini 1976: pg. 88
         *
         * @param {Number} sigma The characteristic SAV ratio [ft2/ft3].
         * @param {Number} beta_ratio The relative packing ratio [beta/beta_opt].
         * @return {Number} gamma [1/min]
         */
        static double reactionVelocity(double sigma, double beta_ratio)
        {
            double sigma15 = Pow(sigma, 1.5);
            double A = 133 / Pow(sigma, 0.7913);    // Albini 
            double gamma_max = sigma15 / (495 + 0.0594 * sigma15);
            double gamma = gamma_max * Pow(beta_ratio, A) * Exp(A * (1 - beta_ratio));

            return gamma;
        }

        /**
         * Calculates the reaction intensity: I_r.
         *
         * The rate of heat release, per unit area of the flaming fire front, expressed as heat
         * energy/area/time, such as Btu/square foot/minute, or Kcal/square meter/second.
         *
         * Rothermel 1972: eq. (58), (59) thru (60)
         *
         * @param {Number} gamma The potential reaction velocity.
         * @param {Number} heat The low heat content [Btu/lb]
         * @param {Number} eta_M The moisture damping coefficient.
         * @param {Number} eta_s The mineral damping coefficient.
         * @return {Number} I_r [BTU/ft2/min].
         */
        static double reactionIntensity(double gamma, double heat, double eta_M, double eta_s)
        {
            double I_r = gamma * heat * eta_M * eta_s;
            return I_r;
        }

        /**
         * Calculates the flame residence time: tau.
         *
         * Albini (1976): p.91
         *
         * @param {Number} sigma The characteristic SAV ratio [ft2/ft3].
         * @return {Number} tau [min].
         */
        static double flameResidenceTime(double sigma)
        {
            if (sigma <= 0)
            {
                //throw new RangeError("sigma must be > 0.")
            }
            double tau = 384 / sigma;
            return tau;
        }

        /**
         * Calculates the heat release per unit area: hpa.
         *
         * @param {Number} I_r The reaction intensity [Btu/ft2/min].
         * @param {Number} tau The flame residence time [min].
         * @return {NUmber} hpa [Btu/ft2]
         */
        static double heatRelease(double I_r, double tau)
        {
            double hpa = I_r * tau;

            return hpa;
        }

        /**
         * Gets the propagating flux ratio: xi.
         *
         * The no-wind propagating flux ratio is a function of the mean packing ratio (beta) and the
         * characteristic SAV ratio (sigma).
         *
         * Rothermel 1972: eq. (42)(76)
         *
         * @param {Number} sigma The characteristic SAV ratio [ft2/ft3].
         * @param {Number} beta The mean packing ratio [-]
         * @return {Number} xi
         */
        static double propagatingFluxRatio(double sigma, double beta)
        {
            if (sigma <= 0)
            {
                //throw new RangeError("sigma must be > 0.")
            }
            double xi = Exp((0.792 + 0.681 * Sqrt(sigma)) * (beta + 0.1)) / (192 + 0.2595 * sigma);

            return xi;
        }

        /**
         * Calculates the effective heating number: epsilon.
         *
         * Rothermel 1972: eq. (14) and (77).
         *
         * @param {Number} sv The SAV ratio value for an individual particle [ft2/ft3].
         *
         * @return {Number} epsilon.
         */
        static double effectiveHeatingNumber(double sv)
        {
            double epsilon = 0;
            if (sv > 0)
            {
                epsilon = Exp(-138 / sv);
            }

            return epsilon;
        }

        /**
         * Calculates the heat of preignition: Q_ig.
         *
         * Rothermel 1972: eq. (12) and (78).
         *
         * @param {Number} Mf The fuel moisture value for an individual fuel particle [%].
         * @return {Number} Q_ig.
         */
        static double heatOfPreignition(double Mf)
        {
            double Q_ig = 250 + 1116 * (Mf * 0.01); // Mf = [fraction]
            return Q_ig;
        }

        /**
         * Calculates the heat sink term: hsk.
         *
         * Rothermel 1972: eq. (77).
         *
         * @param {Number[]} preignitionHeat An array of heat of preignition values for individual particles
         * (Q_ig).
         * @param {Number[]} effectiveHeating An array of effective heating number values for individual particles
         * (epsilon).
         * @param {Number[]} sw An array of (sv * w0) weighting values for individual fuel particles (sw).
         * @param {Number} density The mean bulk density for the fuel complex (rho_b).
         * @return {NUmber} hsk [Btu/ft3]
         */
        static double heatSink(double[] preignitionHeat, double[] effectiveHeating, double[] sw, double density)
        {
            double Qig_t = 0;   // sum[i=1,n][Qig_i]
            double sw_t = 0;    // sum[i=1,n][sw_i]

            for (int i = 0; i < sw.Length; i++)
            {
                Qig_t += preignitionHeat[i] * effectiveHeating[i] * sw[i];
                sw_t += sw[i];
            }
            double hsk = density * (Qig_t / sw_t);

            return hsk;
        }

        /**
         * Calculates the wind factor: phi_w.
         *
         * Rothermel 1972: eq. (47) and (79),(82),(83),(84)
         *
         * @param {Number} midFlameWindSpd The wind speed at mid-flame height [ft/min].
         * @param {Number} sigma The characteristic SAV ratio for the fuelbed [ft2/ft3].
         * @param {Number} beta_ratio The relative packing ratio [beta/beta_opt].
         * @return {Number} phi_w
         */
        static double windFactor(double midFlameWindSpd, double sigma, double beta_ratio)
        {
            double C = Rothermel.windParameterC(sigma);
            double B = Rothermel.windParameterB(sigma);
            double E = Rothermel.windParameterE(sigma);
            double phi_w = Rothermel.windFactor2(midFlameWindSpd, C, B, E, beta_ratio);

            return phi_w;
        }

        /**
         * Calculates the wind multiplier for the rate of spread: phi_w.
         *
         * Rothermel 1972: eq. (47)
         *
         * @param {Number} midFlameWindSpd The wind speed at mid-flame height [ft/min].
         * @param {Number} C Result from Rothermel 1972: eq. (48).
         * @param {Number} B Result from Rothermel 1972: eq. (49).
         * @param {Number} E Result from Rothermel 1972: eq. (50).
         * @param {Number} beta_ratio The relative packing ratio [beta/beta_opt].
         * @return {Number} phi_w
         */
        static double windFactor2(double midFlameWindSpd, double C, double B, double E, double beta_ratio)
        {
            double phi_w = C * Pow(midFlameWindSpd, B) * Pow(beta_ratio, -E);
            return phi_w;
        }

        /**
         * Calculates the wind parameter C.
         *
         * Rothermel 1972: eq. (48)
         *
         * @param {Number} sigma The characteristic SAV ratio for the fuelbed [ft2/ft3].
         * @return {Number} C
         */
        static double windParameterC(double sigma)
        {
            double C = 7.47 * Exp(-0.133 * Pow(sigma, 0.55));
            return C;
        }

        /**
         * Calculates the wind parameter B.
         *
         * Rothermel 1972: eq. (49)
         *
         * @param {Number} sigma The characteristic SAV ratio for the fuelbed [ft2/ft3].
         * @return {Number} B
         */
        static double windParameterB(double sigma)
        {
            double B = 0.02526 * Pow(sigma, 0.54);
            return B;
        }

        /**
         * Calculates the wind parameter E.
         *
         * Rothermel 1972: eq. (50)
         *
         * @param {Number} sigma The characteristic SAV ratio for the fuelbed [ft2/ft3].
         * @return {Number} E
         */
        static double windParameterE(double sigma)
        {
            double E = 0.715 * Exp(-0.000359 * sigma);
            return E;
        }

        /**
         * Calculates the slope multiplier for the rate of spread: phi_s.
         *
         * Rothermel 1972: eq. (51) and (78)
         *
         * @param {Number} slopeDegrees The steepness of the slope [degrees].
         * @param {Number}bbeta The mean packing ratio.
         * @return {Number} phi_s
         */
        static double slopeFactor(double slopeDegrees, double beta)
        {
            double phi = slopeDegrees * TO_RADIANS;
            double tan_phi = Tan(phi);
            double phi_s = 5.275 * Pow(beta, -0.3) * Pow(tan_phi, 2);

            return phi_s;
        }

        /**
         * Calculates the effective wind speed from the combined wind and slope factors: efw.
         *
         * Rothermel 1972: eq. (87)
         *
         * @param {NUmber} phiEw The combined wind and slope factors [phiW + phiS].
         * @param {NUmber} beta_ratio beta/beta_opt.
         * @param {NUmber} sigma The characteristic SAV ratio [ft2/ft3].
         * @return {NUmber} efw [ft/min]
         */
        static double effectiveWindSpeed(double phiEw, double beta_ratio, double sigma)
        {
            double C = Rothermel.windParameterC(sigma);
            double B = Rothermel.windParameterB(sigma);
            double E = Rothermel.windParameterE(sigma);
            double efw = Rothermel.effectiveWindSpeed2(phiEw, C, B, E, beta_ratio);

            return efw;
        }

        /**
         * Calculates the effective wind speed from the combined wind and slope factors: efw.
         *
         * Rothermel 1972: eq. (87)
         *
         * @param {NUmber} phiEw The combined wind and slope factors [phiW + phiS].
         * @param {NUmber} C Result from Rothermel 1972: eq. (48).
         * @param {NUmber} B Result from Rothermel 1972: eq. (49).
         * @param {NUmber} E Result from Rothermel 1972: eq. (50).
         * @param {NUmber} beta_ratio
         * @return {NUmber} efw [ft/min]
         */
        static double effectiveWindSpeed2(double phiEw, double C, double B, double E, double beta_ratio)
        {
            // Effective windspeed: actually this is only the inverse function of phi_w
            double efw = (Pow(phiEw / (C * Pow(beta_ratio, -E)), 1 / B));

            return efw;
        }

        /**
         * Calculates the rate of spread with wind and/or slope: ros.
         *
         * Rothermel 1972: eq. (52) - heat source / heat sink
         *
         * @param {Number} reactionIntensity The fire reaction intensity (I_r) [BTU/ft2/min].
         * @param {Number} propogatingFlux The fire propagating flux (xi) [fraction].
         * @param {Number} windFactor The wind coefficient (phi_w).
         * @param {Number} slopeFactor The slope coefficient (phi_s).
         * @param {Number} heatSink The total heat sink (hsk) [Btu/ft3].
         * @return {Number} ros [ft/min]
         */
        static double rateOfSpread(double reactionIntensity, double propogatingFlux, double windFactor, double slopeFactor, double heatSink)
        {
            if (heatSink <= 0)
            {
                //throw new RangeError("heatSink must be > 0.");
            }
            double ros = (reactionIntensity * propogatingFlux * (1 + windFactor + slopeFactor)) / heatSink;
            
            return ros;
        }

        /**
         * Calculates the rate of spread without wind and slope: ros.
         *
         * Rothermel 1972: eq. (52) - heat source / heat sink
         *
         * @param {Number} reactionIntensity The fire reaction intensity (I_r) [BTU/ft2/min].
         * @param {Number} propogatingFlux The fire propagating flux (xi) [fraction].
         * @param {Number} heatSink The total heat sink (hsk) [Btu/ft3].
         * @return {Number} ros [ft/min]
         */
        static double rateOfSpreadNoWindNoSlope(double reactionIntensity, double propogatingFlux, double heatSink)
        {
            if (heatSink <= 0)
            {
                //throw new RangeError("heatSink must be > 0.")
            }
            double ros = (reactionIntensity * propogatingFlux) / heatSink;

            return ros;
        }

        /**
         * Calculates the flame zone depth: fzd.
         *
         * The depth, or front-to-back distance, of the actively flaming zone of a free spreading fire
         * can be determined from the rate of spread and the particle-residence time. Albini 1976: pg.
         * 86
         *
         * @param {Number} rateOfSpread The fire rate of spread (ros) [ft/min].
         * @param {Number} flameResidenceTime The fuelbed's flame residence time [min].
         * @return {Number} fzd [ft]
         */
        static double flameZoneDepth(double rateOfSpread, double flameResidenceTime)
        {
            double fzd = rateOfSpread * flameResidenceTime;
            return fzd;
        }

        /**
         * Calculates Byram's fireline intensity: I.
         *
         * Byram's intensity, I, is the rate of heat release per unit of fire edge. The reaction
         * intensity, I_r, provided by Rothermel's spread model is the rate of energy release per unit
         * area in the actively flaming zone.
         *
         * Albini 1976: eq. (16), pg. 86
         *
         * @param flameZoneDepth The depth of the actively flaming zone [ft].
         * @param reactionIntensity The fuelbed's fire reaction intensity (I_r) [Btu/ft2/min].
         * @return I [Btu/ft/s]
         */
        static double firelineIntensity(double flameZoneDepth, double reactionIntensity)
        {
            double I = reactionIntensity * flameZoneDepth / 60;

            return I;
        }

        /**
         * Calculates flame length: L.
         *
         * Albini 1976: eq. (17) pg. 86
         *
         * @param {Number} firelineIntensity Byram's fireline intensity (I) [Btu/ft/s].
         * @return {Number} L [ft]
         */
        static double flameLength(double firelineIntensity)
        {
            double L = 0.45 * Pow(firelineIntensity, 0.46);

            return L;
        }

        /**
         * Calculates the wind adjustment factor for scaling wind speed from 20-ft to midflame height.
         *
         * Wind adjustment factor is calculated as an average from the top of the fuel bed to twice the
         * fuel bed depth, using Albini and Baughman (1979) equation 9 (page 5).
         *
         * @param {Number} fuelDepth Fuel bed depth (height) [ft].
         * @return {Number} Wind adjustment factor, waf [0..1]
         */
        static double midFlameWindAdjustmentFactor(double fuelDepth)
        {
            double waf = 1.0;
            if(fuelDepth > 0)
            {
                // From BehavePlus5, xfblib.cpp by Collin D. Bevins
                waf = 1.83 / Log((20 + 0.36 * fuelDepth) / (0.13 * fuelDepth));
            }

            return Min(Max(waf, 0), 1);
        }

        /**
         * Computes the mid-flame wind speed from 20 foot wind speeds. Used to compute rate of spread.
         *
         * @param {Number} wndSpd20Ft Wind speed 20 feet above the vegetation [MPH]
         * @param {Number} fuelDepth Vegetation height [feet]
         * @return {Number} Wind speed at vegetation height
         */
        static double calcWindSpeedMidFlame(double wndSpd20Ft, double fuelDepth)
        {
            return wndSpd20Ft * Rothermel.midFlameWindAdjustmentFactor(fuelDepth);
        }

        /**
         * Computes the wind speed at the fuel level from 20 foot wind speeds. Used to compute wind
         * cooling effect on fuel temperatures.
         *
         * @param {Number} wndSpd20Ft Wind speed 20 feet above the vegetation [MPH]
         * @param {Number} fuelDepth Vegetation height [feet]
         * @return {Number} Wind speed at vegetation height
         */
        static double calcWindSpeedNearFuel(double wndSpd20Ft, double fuelDepth)
        {
            // Equation #36
            // The ratio of windspeed at vegetation height to that at
            // 20 feet above the vegitation is given by:
            //  U_h' / U_20+h' = 1 / ln((20 + 0.36 * h') / 0.13 * h')
            //      where:
            //          h' = vegitation height [feet]
            if (fuelDepth == 0)
            {
                fuelDepth = 0.1;
            }
            double U_h = (1.0 / Log((20 + 0.36 * fuelDepth) / (0.13 * fuelDepth))) * wndSpd20Ft;

            return U_h;
        }

        /**
         * Calculates the fire ellipse eccentricity from the effective wind speed.
         *
         * Anderson 1983: eq. (4)
         *
         * <pre>
         * Consider using Anderson 1983: eq. (17)
         *      l/w = 0.936 EXP(0.1l47U) + 0.461 EXP(-0.0692U)
         *  where U = midflame miles per hour.
         * </pre>
         * 
         * @param {Number} effectiveWind The effective wind speed of the combined wind and slope. [mph]
         * @return {Number} The eccentricity of the ellipse
         */
        static double eccentricity(double effectiveWind)
        {
            double eccentricity = 0;
            if (effectiveWind > 0)
            {
                // From FireLib 1.04, firelib.c by Collin D. Bevins
                // a1 = major axis of semiellipse at the rear of the fire,
                //    = 1. + 0.25 * effectiveWindSpd / 88.0);
                double lbRatio = 1.0 + 0.002840909 * effectiveWind;
                if (lbRatio > 1.00001)
                {
                    eccentricity = Sqrt(Pow(lbRatio, 2) - 1.0) / lbRatio;
                }
            }

            return eccentricity;
        }
    }
    }
