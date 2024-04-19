//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.BehaveUnits;

namespace WUIPlatform.Fire
{
    public class CrownInputs
    {
        double canopyBaseHeight_; //Canopy base height(ft)
        double canopyBulkDensity_; // Canopy bulk density(lb / ft3)
        double canopyUserProvidedFlameLength_; // Flame length(ft)
        double canopyUserProvidedFirelineIntensity_; // Fireline intensity(ft)
        double moistureFoliar_; // Tree foliar moisture content (lb water/lb foliage)

        public CrownInputs()
        {
            initializeMembers();
        }

        ~CrownInputs()
        {

        }

        public void initializeMembers()
        {
            canopyBaseHeight_ = 0;
            canopyBulkDensity_ = 0;
            canopyUserProvidedFlameLength_ = 0;
            canopyUserProvidedFirelineIntensity_ = 0;
            moistureFoliar_ = 0;
        }

        public double getCanopyBaseHeight(LengthUnits.LengthUnitsEnum heightUnits)
        {
            return LengthUnits.fromBaseUnits(canopyBaseHeight_, heightUnits);
        }

        public double getCanopyBulkDensity(DensityUnits.DensityUnitsEnum densityUnits)
        {
            return DensityUnits.fromBaseUnits(canopyBulkDensity_, densityUnits);
        }

        public double getCanopyFlameLength()
        {
            return canopyUserProvidedFlameLength_;
        }

        public double getCanopyFirelineIntensity()
        {
            return canopyUserProvidedFirelineIntensity_;
        }

        public double getMoistureFoliar(MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            return MoistureUnits.fromBaseUnits(moistureFoliar_, moistureUnits);
        }

        public void setCanopyBaseHeight(double canopyBaseHeight, LengthUnits.LengthUnitsEnum heightUnits)
        {
            canopyBaseHeight_ = LengthUnits.toBaseUnits(canopyBaseHeight, heightUnits);
        }

        public void setCanopyBulkDensity(double canopyBulkDensity, DensityUnits.DensityUnitsEnum densityUnits)
        {
            canopyBulkDensity_ = DensityUnits.toBaseUnits(canopyBulkDensity, densityUnits);
        }

        public void setCanopyFlameLength(double canopyUserProvidedFlameLength)
        {
            canopyUserProvidedFlameLength_ = canopyUserProvidedFlameLength;
        }

        public void setCanopyFirelineIntensity(double canopyUserProvidedFirelineIntensity)
        {
            canopyUserProvidedFirelineIntensity_ = canopyUserProvidedFirelineIntensity;
        }

        public void setMoistureFoliar(double moistureFoliar, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            moistureFoliar_ = MoistureUnits.toBaseUnits(moistureFoliar, moistureUnits);
        }

        public void updateCrownInputs(double canopyBaseHeight, LengthUnits.LengthUnitsEnum canopyBaseHeightUnits,
            double canopyBulkDensity, DensityUnits.DensityUnitsEnum densityUnits,
            double moistureFoliar, MoistureUnits.MoistureUnitsEnum moistureUnits)
        {
            setCanopyBaseHeight(canopyBaseHeight, canopyBaseHeightUnits);
            setCanopyBulkDensity(canopyBulkDensity, densityUnits);
            setMoistureFoliar(moistureFoliar, moistureUnits);
        }
    }
}


