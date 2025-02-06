//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire.Behave
{
    public class PalmettoGallberry
    {
        double moistureOfExtinctionDead_;
        double heatOfCombustionDead_;
        double heatOfCombustionLive_;
        double palmettoGallberyDeadOneHourLoad_;
        double palmettoGallberyDeadTenHourLoad_;
        double palmettoGallberyDeadFoliageLoad_;
        double palmettoGallberyFuelBedDepth_;
        double palmettoGallberyLitterLoad_;
        double palmettoGallberyLiveOneHourLoad_;
        double palmettoGallberyLiveTenHourLoad_;
        double palmettoGallberyLiveFoliageLoad_;

        public PalmettoGallberry()
        {
            initializeMembers();
        }

        public void initializeMembers()
        {
            moistureOfExtinctionDead_ = 0.40;
            heatOfCombustionDead_ = 8300;
            heatOfCombustionLive_ = 8300;
            palmettoGallberyDeadOneHourLoad_ = 0.0;
            palmettoGallberyDeadTenHourLoad_ = 0.0;
            palmettoGallberyDeadFoliageLoad_ = 0.0;
            palmettoGallberyFuelBedDepth_ = 0.0;
            palmettoGallberyLitterLoad_ = 0.0;
            palmettoGallberyLiveOneHourLoad_ = 0.0;
            palmettoGallberyLiveTenHourLoad_ = 0.0;
            palmettoGallberyLiveFoliageLoad_ = 0.0;
        }

        public double calculatePalmettoGallberyDeadOneHourLoad(double ageOfRough, double heightOfUnderstory)
        {
            palmettoGallberyDeadOneHourLoad_ = -0.00121
                + 0.00379 * log(ageOfRough)
                + 0.00118 * heightOfUnderstory * heightOfUnderstory;
            if (palmettoGallberyDeadOneHourLoad_ < 0.0)
            {
                palmettoGallberyDeadOneHourLoad_ = 0.0;
            }
            return palmettoGallberyDeadOneHourLoad_;
        }

        public double calculatePalmettoGallberyDeadTenHourLoad(double ageOfRough, double palmettoCoverage)
        {
            palmettoGallberyDeadTenHourLoad_ = -0.00775
                + 0.00021 * palmettoCoverage
                + 0.00007 * ageOfRough * ageOfRough;
            if (palmettoGallberyDeadTenHourLoad_ < 0.0)
            {
                palmettoGallberyDeadTenHourLoad_ = 0.0;
            }
            return palmettoGallberyDeadTenHourLoad_;
        }

        public double calculatePalmettoGallberyDeadFoliageLoad(double ageOfRough, double palmettoCoverage)
        {
            palmettoGallberyDeadFoliageLoad_ = 0.00221 * pow(ageOfRough, 0.51263) * exp(0.02482 * palmettoCoverage);
            return palmettoGallberyDeadFoliageLoad_;
        }

        public double calculatePalmettoGallberyFuelBedDepth(double heightOfUnderstory)
        {
            palmettoGallberyFuelBedDepth_ = 2.0 * heightOfUnderstory / 3.0;
            return palmettoGallberyFuelBedDepth_;
        }

        public double calculatePalmettoGallberyLitterLoad(double ageOfRough, double overstoryBasalArea)
        {
            palmettoGallberyLitterLoad_ = (0.03632 + 0.0005336 * overstoryBasalArea) * (1.0 - pow(0.25, ageOfRough));
            return palmettoGallberyLitterLoad_;
        }

        public double calculatePalmettoGallberyLiveOneHourLoad(double ageOfRough, double heightOfUnderstory)
        {
            palmettoGallberyLiveOneHourLoad_ = 0.00546 + 0.00092 * ageOfRough + 0.00212 * heightOfUnderstory * heightOfUnderstory;
            return palmettoGallberyLiveOneHourLoad_;
        }

        public double calculatePalmettoGallberyLiveTenHourLoad(double ageOfRough, double heightOfUnderstory)
        {
            palmettoGallberyLiveTenHourLoad_ = -0.02128
                + 0.00014 * ageOfRough * ageOfRough
                + 0.00314 * heightOfUnderstory * heightOfUnderstory;
            if (palmettoGallberyLiveTenHourLoad_ < 0.0)
            {
                palmettoGallberyLiveTenHourLoad_ = 0.0;
            }
            return palmettoGallberyLiveTenHourLoad_;
        }

        public double calculatePalmettoGallberyLiveFoliageLoad(double ageOfRough, double palmettoCoverage,
            double heightOfUnderstory)
        {
            palmettoGallberyLiveFoliageLoad_ = -0.0036
                + 0.00253 * ageOfRough
                + 0.00049 * palmettoCoverage
                + 0.00282 * heightOfUnderstory * heightOfUnderstory;
            if (palmettoGallberyLiveFoliageLoad_ < 0.0)
            {
                palmettoGallberyLiveFoliageLoad_ = 0.0;
            }
            return palmettoGallberyLiveFoliageLoad_;
        }

        public double getMoistureOfExtinctionDead()
        {
            return moistureOfExtinctionDead_;
        }

        public double getHeatOfCombustionDead()
        {
            return heatOfCombustionDead_;
        }

        public double getHeatOfCombustionLive()
        {
            return heatOfCombustionLive_;
        }

        public double getPalmettoGallberyDeadOneHourLoad()
        {
            return palmettoGallberyDeadOneHourLoad_;
        }

        public double getPalmettoGallberyDeadTenHourLoad()
        {
            return palmettoGallberyDeadTenHourLoad_;
        }

        public double getPalmettoGallberyDeadFoliageLoad()
        {
            return palmettoGallberyDeadFoliageLoad_;
        }

        public double getPalmettoGallberyFuelBedDepth()
        {
            return palmettoGallberyFuelBedDepth_;
        }

        public double getPalmettoGallberyLitterLoad()
        {
            return palmettoGallberyLitterLoad_;
        }

        public double getPalmettoGallberyLiveOneHourLoad()
        {
            return palmettoGallberyLiveOneHourLoad_;
        }

        public double getPalmettoGallberyLiveTenHourLoad()
        {
            return palmettoGallberyLiveTenHourLoad_;
        }

        public double getPalmettoGallberyLiveFoliageLoad()
        {
            return palmettoGallberyLiveFoliageLoad_;
        }
}
}

