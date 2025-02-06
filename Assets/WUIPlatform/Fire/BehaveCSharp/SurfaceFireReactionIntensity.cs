//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.MathWrap;
using static WUIPlatform.Fire.Behave.BehaveUnits;

namespace WUIPlatform.Fire.Behave
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

