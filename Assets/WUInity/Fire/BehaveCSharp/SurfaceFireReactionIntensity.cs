/******************************************************************************
*
* Project:  CodeBlocks
* Purpose:  Class for calculating surface fire reaction intensity
* Author:   William Chatham <wchatham@fs.fed.us>
* Credits:  Some of the code in the corresponding cpp file is, in part or in
*           whole, from BehavePlus5 source originally authored by Collin D.
*           Bevins and is used with or without modification.
*
*******************************************************************************
*
* THIS SOFTWARE WAS DEVELOPED AT THE ROCKY MOUNTAIN RESEARCH STATION (RMRS)
* MISSOULA FIRE SCIENCES LABORATORY BY EMPLOYEES OF THE FEDERAL GOVERNMENT
* IN THE COURSE OF THEIR OFFICIAL DUTIES. PURSUANT TO TITLE 17 SECTION 105
* OF THE UNITED STATES CODE, THIS SOFTWARE IS NOT SUBJECT TO COPYRIGHT
* PROTECTION AND IS IN THE PUBLIC DOMAIN. RMRS MISSOULA FIRE SCIENCES
* LABORATORY ASSUMES NO RESPONSIBILITY WHATSOEVER FOR ITS USE BY OTHER
* PARTIES,  AND MAKES NO GUARANTEES, EXPRESSED OR IMPLIED, ABOUT ITS QUALITY,
* RELIABILITY, OR ANY OTHER CHARACTERISTIC.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*
******************************************************************************/
using static WUIEngine.Fire.MathWrap;
using static WUIEngine.Fire.BehaveUnits;

namespace WUIEngine.Fire
{
    public class SurfaceFireReactionIntensity
    {
        double[] etaM_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                            // Moisture damping coefficient for  i-th categort (dead/live)
        double[] etaS_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];                            // Mineral(silica) damping coefficient for i - th categort(dead / live)
        double[] reactionIntensityForLifeState_ = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];   // Reaction intensity for i-th category (dead/live)
        double reactionIntensity_;                                                              // Reaction Intensity, Rothermel 1972, equation 27 (Btu/ft2/min)

        SurfaceFuelbedIntermediates surfaceFuelbedIntermediates_;

        public SurfaceFireReactionIntensity()
        {

        }

        public SurfaceFireReactionIntensity(SurfaceFuelbedIntermediates surfaceFuelbedIntermediates)
        {
            surfaceFuelbedIntermediates_ = surfaceFuelbedIntermediates;
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                etaM_[i] = 0.0;
                etaS_[i] = 0.0;
                reactionIntensityForLifeState_[i] = 0.0;
            }
            reactionIntensity_ = 0.0;
        }

        public SurfaceFireReactionIntensity(SurfaceFireReactionIntensity rhs)
        {
            memberwiseCopyAssignment(rhs);
        }

        /*SurfaceFireReactionIntensity& operator=(const SurfaceFireReactionIntensity& rhs)
        {
            if (this != &rhs)
            {
                memberwiseCopyAssignment(rhs);
            }
            return *this;
        }*/

        void memberwiseCopyAssignment(SurfaceFireReactionIntensity rhs)
        {
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                etaM_[i] = rhs.etaM_[i];
                etaS_[i] = rhs.etaS_[i];
                reactionIntensityForLifeState_[i] = rhs.reactionIntensityForLifeState_[i];
            }
            reactionIntensity_ = rhs.reactionIntensity_;
        }

        public double calculateReactionIntensity()
        {
            double aa = 0.0; // Alternate "arbitrary variable" A value for Rothermel equations for use in computer models, Albini 1976, p. 88
            reactionIntensity_ = 0.0;  // Reaction Intensity, Rothermel 1972, equation 27

            double sigma = surfaceFuelbedIntermediates_.getSigma();
            double relativePackingRatio = surfaceFuelbedIntermediates_.getRelativePackingRatio();

            aa = 133.0 / pow(sigma, 0.7913);

            //double gammaMax = (sigma * sqrt(sigma)) / (495.0 + (.0594 * sigma * sqrt(sigma)));
            double sigmaToTheOnePointFive = pow(sigma, 1.5);
            double gammaMax = sigmaToTheOnePointFive / (495.0 + (0.0594 * sigmaToTheOnePointFive));
            double gamma = gammaMax * pow(relativePackingRatio, aa) * exp(aa * (1.0 - relativePackingRatio));

            double[] weightedFuelLoad = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];
            weightedFuelLoad[(int)SurfaceInputs.FuelConstants.DEAD] = surfaceFuelbedIntermediates_.getWeightedFuelLoadByLifeState((int)SurfaceInputs.FuelConstants.DEAD);
            weightedFuelLoad[(int)SurfaceInputs.FuelConstants.LIVE] = surfaceFuelbedIntermediates_.getWeightedFuelLoadByLifeState((int)SurfaceInputs.FuelConstants.LIVE);

            double[] weightedHeat = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];
            weightedHeat[(int)SurfaceInputs.FuelConstants.DEAD] = surfaceFuelbedIntermediates_.getWeightedHeatByLifeState((int)SurfaceInputs.FuelConstants.DEAD);
            weightedHeat[(int)SurfaceInputs.FuelConstants.LIVE] = surfaceFuelbedIntermediates_.getWeightedHeatByLifeState((int)SurfaceInputs.FuelConstants.LIVE);

            calculateEtaM();
            calculateEtaS();

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                reactionIntensityForLifeState_[i] = gamma * weightedFuelLoad[i] * weightedHeat[i] * etaM_[i] * etaS_[i];
            }
            reactionIntensity_ = reactionIntensityForLifeState_[(int)SurfaceInputs.FuelConstants.DEAD] + reactionIntensityForLifeState_[(int)SurfaceInputs.FuelConstants.LIVE];

            return reactionIntensity_;
        }

        public void calculateEtaM()
        {
            double relativeMoisture = 0;	// (Moisture content) / (Moisture of extinction)

            double[] weightedMoisture = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];
            weightedMoisture[(int)SurfaceInputs.FuelConstants.DEAD] = surfaceFuelbedIntermediates_.getWeightedMoistureByLifeState((int)SurfaceInputs.FuelConstants.DEAD);
            weightedMoisture[(int)SurfaceInputs.FuelConstants.LIVE] = surfaceFuelbedIntermediates_.getWeightedMoistureByLifeState((int)SurfaceInputs.FuelConstants.LIVE);

            double[] moistureOfExtinction = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];
            moistureOfExtinction[(int)SurfaceInputs.FuelConstants.DEAD] = surfaceFuelbedIntermediates_.getMoistureOfExtinctionByLifeState((int)SurfaceInputs.FuelConstants.DEAD);
            moistureOfExtinction[(int)SurfaceInputs.FuelConstants.LIVE] = surfaceFuelbedIntermediates_.getMoistureOfExtinctionByLifeState((int)SurfaceInputs.FuelConstants.LIVE);

            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                if (moistureOfExtinction[i] > 0.0)
                {
                    relativeMoisture = weightedMoisture[i] / moistureOfExtinction[i];
                }
                if (weightedMoisture[i] >= moistureOfExtinction[i] || relativeMoisture > 1.0)
                {
                    etaM_[i] = 0.0;
                }
                else
                {
                    etaM_[i] = 1.0 - (2.59 * relativeMoisture) + (5.11 * relativeMoisture * relativeMoisture) - (3.52 * relativeMoisture * relativeMoisture * relativeMoisture);
                }
            }
        }

        public void calculateEtaS()
        {
            double[] weightedSilica = new double[(int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES];
            weightedSilica[(int)SurfaceInputs.FuelConstants.DEAD] = surfaceFuelbedIntermediates_.getWeightedSilicaByLifeState((int)SurfaceInputs.FuelConstants.DEAD);
            weightedSilica[(int)SurfaceInputs.FuelConstants.LIVE] = surfaceFuelbedIntermediates_.getWeightedSilicaByLifeState((int)SurfaceInputs.FuelConstants.LIVE);

            double etaSDenomitator = 0;
            for (int i = 0; i < (int)SurfaceInputs.FuelConstants.MAX_LIFE_STATES; i++)
            {
                etaSDenomitator = pow(weightedSilica[i], 0.19);
                if (etaSDenomitator < 1e-6)
                {
                    etaS_[i] = 0.0;
                }
                else
                {
                    etaS_[i] = 0.174 / etaSDenomitator; // 0.174 / pow(weightedSilica[i], 0.19)
                }
                if (etaS_[i] > 1.0)
                {
                    etaS_[i] = 1.0;
                }
            }
        }

        public double getReactionIntensity(HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum reactiontionIntensityUnits)
        {
            return reactionIntensity_;
        }
    }
}

