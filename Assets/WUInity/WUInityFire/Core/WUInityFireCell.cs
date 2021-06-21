using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.Fire.MathWrap;
using System.Runtime.InteropServices;
using UnityEditor;

namespace WUInity.Fire
{  
    //[System.Serializable]
    public class WUInityFireCell
    {       
        public Vector2Int cellIndex;
        public FireCellState cellState;

        //firefront is the different spread directions and how far they have travelled
        double[] fireFront; //gets set to -1 when done spreading, otherwise always positive
        //this is teh distance to the next cell center, so when firefront has passed distance whe have spread
        double[] distances;
        double[] spreadRates;
        WUInityFireCell[] neighbors;
        WUInityFireMesh fireMesh;
        LandScapeStruct lcp;
        double maxSpreadRate;
        double maxSpreadRateDirection;
        double[] maxSpreadRates; 
        double fireLineIntensity;
        double maxFireLineIntensity;
        double timeOfArrival;
        double heatPerUnitArea;

        //calls BEHAVE DLL
        [DllImport("BEHAVEDLL", EntryPoint = "CalcFireMaxSpreadRate")]
        public static extern double CalcFireMaxSpreadRate(int fuelModelNumber,
            double moistureOneHour, double moistureTenHour, double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody,
            double windSpeed, double windDirection,
            double slope, double aspect,
            double directionOfInterest,
            double canopyCover, double canopyHeight, double crownRatio);
        

        public WUInityFireCell(WUInityFireMesh fireMesh, int xIndex, int yIndex, LandScapeStruct lcp)
        {
            this.fireMesh = fireMesh;
            cellIndex = new Vector2Int(xIndex, yIndex);
            cellState = FireCellState.CanBurn;            

            fireFront = new double[fireMesh.indexSize];
            distances = new double[fireMesh.indexSize];
            spreadRates = new double[fireMesh.indexSize];
            maxSpreadRates = new double[fireMesh.indexSize];
            neighbors = new WUInityFireCell[fireMesh.indexSize];

            this.lcp = lcp;       
        }

        /// <summary>
        /// Sets neightbors, calculates distance to neighbor cell centers and calculates slope and aspect
        /// </summary>
        public void InitCell()
        {            
            //calc distance to next cell center in current direction
            for (int i = 0; i < distances.Length; i++)
            {
                int xI = cellIndex.x + fireMesh.neighborIndices[i].x;
                int yI = cellIndex.y + fireMesh.neighborIndices[i].y;                
                if (fireMesh.IsInsideMesh(xI, yI) && fireMesh.GetFireCell(xI, yI).cellState == FireCellState.CanBurn)
                {
                    WUInityFireCell f = fireMesh.GetFireCell(xI, yI);
                    //set neighbor since it exists
                    neighbors[i] = f;
                    
                    //caclulate actual distance
                    double dist;
                    if(i < 4)
                    {
                        dist = fireMesh.cellSizeSquared;
                    }
                    else if (i < 8)
                    {
                        dist = fireMesh.cellSizeDiagonalSquared;
                    }
                    else
                    {
                        dist = fireMesh.sixteenDistSquared;
                    }
                    //assumes x = y cell size
                    distances[i] = sqrt(2 * dist + pow2(lcp.elevation- f.lcp.elevation));
                }
                else
                {
                    fireFront[i] = -1.0;
                    distances[i] = -1.0;                    
                }
                maxSpreadRates[i] = 0.0;
            }
            if(lcp.slope == -1 && lcp.aspect == -1)
            {
                CalculateSlopeAndAspect();
            }                      
            maxSpreadRate = -1.0;                 
        }

        public void Ignite(double timeOfArrival)
        {
            this.timeOfArrival = timeOfArrival;
            if (cellState == FireCellState.CanBurn)
            {
                cellState = FireCellState.Burning;

                //initialize spread rate if not done before
                if (maxSpreadRate < 0.0)
                {
                    UpdateSpreadRates();
                }
            }
        }

        public void QueueIgnition(int directionIndex, double firefrontSpill)
        {
            if (cellState == FireCellState.CanBurn)
            {
                //add spill (remainder of fire crossing center) of firespread
                fireFront[directionIndex] = firefrontSpill;
                fireMesh.AddCellToIgnite(this);
            }
        }

        private void CalculateSlopeAndAspect()
        {
            //https://www.asprs.org/wp-content/uploads/pers/1987journal/aug/1987_aug_1109-1111.pdf
            double[] neighborElevations = new double[4];
            for (int i = 0; i < 4; ++i)
            {
                neighborElevations[i] = lcp.elevation;
                if (neighbors[i] != null)
                {
                    neighborElevations[i] = neighbors[i].lcp.elevation;
                }
            }
            double e1 = neighborElevations[3];
            double e2 = neighborElevations[0];
            double e3 = neighborElevations[1];
            double e4 = neighborElevations[2];
            double d = fireMesh.cellSize.x;

            Vector3D n = new Vector3D(e1 - e3, e4 - e2, 2.0 * d);
            lcp.slope = (short)(0.5 + 100.0 * sqrt(n.x * n.x + n.y * n.y) / n.z);

            if(lcp.slope == 0)
            {
                lcp.aspect = 0;
            }
            else if(n.x == 0.0)
            {
                if (n.y < 0.0)
                {
                    lcp.aspect = 180;
                }
                else
                {
                    lcp.aspect = 360;
                }
            }
            else if(n.x > 0.0)
            {
                lcp.aspect = (short)(0.5 + 90.0 - 57.296 * atan(n.y / n .x));
            }
            else if(n.x < 0.0)
            {
                lcp.aspect = (short)(0.5 + 270.0 - 57.296 * atan(n.y / n.x));
            }
            //MonoBehaviour.print("Slope: " + slope + ", Aspect: " + aspect);
        }
        
        public int GetFuelModelNumber()
        {
            return lcp.fuel_model;
        }

        public double GetSlope()
        {
            return lcp.slope;
        }

        public double GetAspect()
        {
            return lcp.aspect;
        }

        public double GetElevation()
        {
            return lcp.elevation;
        }

        public double GetFireLineIntensity(bool getMax)
        {
            if(getMax)
            {
                return maxFireLineIntensity;
            }
            else
            {
                return fireLineIntensity;
            }            
        }

        public WUInityFireMesh GetMesh()
        {
            return fireMesh;
        }

        public short GetCanopyCover()
        {
            return lcp.canopy_cover;
        }

        public void Burn()
        {
            if(cellState != FireCellState.Burning)
            {
                return;
            }  
            UpdateFireFront();
        }

        public double GetMaxSpreadRate()
        {
            return maxSpreadRate;
        }

        public void UpdateSpreadRates()
        {
            UpdateCrownFireSpreadRates();
            return;

            FuelMoisture moisture = fireMesh.initialFuelMoisture.GetInitialFuelMoisture(lcp.fuel_model);
            double moistureOneHour = moisture.OneHour;// 6.0;
            double moistureTenHour = moisture.TenHour; //7.0;
            double moistureHundredHour = moisture.HundredHour; //8.0;
            double moistureLiveHerbaceous = moisture.LiveHerbaceous; //60.0;
            double moistureLiveWoody = moisture.LiveWoody; //90.0;

            int fuelModelNumber = lcp.fuel_model;
            int slope = lcp.slope;
            int aspect = lcp.aspect;

            double windSpeed = fireMesh.currentWindData.speed;
            double windDirection = fireMesh.currentWindData.direction;
            // Wind adjustment factor parameters
            double canopyCover = lcp.canopy_cover;
            double canopyHeight = lcp.canopy_height;
            double crownRatio = lcp.bulk_density; //TODO: is this correct?

            //C#
            TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
            BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
            WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
            BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
            BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
            BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
            BehaveUnits.SpeedUnits.SpeedUnitsEnum speedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
            WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth; //fireMesh.surfaceFire.getWindAndSpreadOrientationMode(); //WindAndSpreadOrientationMode.RelativeToUpslope;

            //feed new data
            fireMesh.surfaceFire.updateSurfaceInputs(fuelModelNumber,
                moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureUnits,
                windSpeed, speedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode,
                slope, slopeUnits, aspect,
                canopyCover, coverUnits, canopyHeight, lengthUnits, crownRatio);

            //do new calc
            fireMesh.surfaceFire.doSurfaceRunInDirectionOfMaxSpread();
            double currentMaxSpreadRate = fireMesh.surfaceFire.getSpreadRate(speedUnits);
            double directionOfMaxSpread = fireMesh.surfaceFire.getDirectionOfMaxSpread();            
            if (currentMaxSpreadRate > maxSpreadRate)
            {
                maxSpreadRate = currentMaxSpreadRate;
                maxSpreadRateDirection = directionOfMaxSpread;
            }

            double eccentricity_ = fireMesh.surfaceFire.getFireEccentricity();

            fireLineIntensity = fireMesh.surfaceFire.getFirelineIntensity(BehaveUnits.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilojoulesPerMeterPerSecond);
            if(fireLineIntensity > maxFireLineIntensity)
            {
                maxFireLineIntensity = fireLineIntensity;
            }
            heatPerUnitArea = fireMesh.surfaceFire.getHeatPerUnitArea() * 11.356538527057; //comes in btu/sq.feet, now in kJ/m2
            //MonoBehaviour.print(heatPerUnitArea * 11.356538527057 + " kJ/m2");

            //we have the max spread rate, but need in all directions       
            for (int i = 0; i < spreadRates.Length; i++)
            {
                //not spreading in this direction
                if (neighbors[i] == null)
                {
                    continue;
                }
                //set default
                spreadRates[i] = currentMaxSpreadRate;

                //TODO: use this instead? have to get units and directions right...
                //spreadRates[i] = fireMesh.surfaceFire.calculateSpreadRateAtVector(directionOfInterest); 

                //TODO: canreplaced by calculateSpreadRateAtVector()
                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                // Calcualte beta: the angle between the direction of max spread and the direction of interest
                double directionOfInterest = RadToDegrees(fireMesh.GetAngleOffset(i));
                double beta = abs(directionOfMaxSpread - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (fabs(beta) > 0.1)
                {
                    double radians = beta * M_PI / 180.0;
                    spreadRates[i] = currentMaxSpreadRate * (1.0 - eccentricity_) / (1.0 - eccentricity_ * cos(radians));// * Random.Range(0.8f, 1.2f);                    
                }

                if (spreadRates[i] > maxSpreadRates[i])
                {
                    maxSpreadRates[i] = spreadRates[i];
                }
            }

            //call C++ instead?
            /*maxSpreadRate = CalcFireMaxSpreadRate(fuelModelNumber, 
                moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody,
                windSpeed, windDirection,
                slope, aspect, directionOfInterest,
                canopyCover, canopyHeight, crownRatio);*/
        }

        /// <summary>
        /// This replaces surface run as it does BOTH crown AND surface runs and save each part.
        /// </summary>
        void UpdateCrownFireSpreadRates()
        {
            FuelMoisture moisture = fireMesh.initialFuelMoisture.GetInitialFuelMoisture(lcp.fuel_model);
            double moistureOneHour = moisture.OneHour;// 6.0;
            double moistureTenHour = moisture.TenHour; //7.0;
            double moistureHundredHour = moisture.HundredHour; //8.0;
            double moistureLiveHerbaceous = moisture.LiveHerbaceous; //60.0;
            double moistureLiveWoody = moisture.LiveWoody; //90.0;

            //TODO: what is this?
            double moistureFoliar = 0.0;

            int fuelModelNumber = lcp.fuel_model;
            int slope = lcp.slope;
            int aspect = lcp.aspect;
            double canopyBaseHeight = lcp.canopy_height;
            double canopyBulkDensity = lcp.bulk_density;


            double windSpeed = fireMesh.currentWindData.speed;
            double windDirection = fireMesh.currentWindData.direction;
            // Wind adjustment factor parameters
            double canopyCover = lcp.canopy_cover;
            double canopyHeight = lcp.canopy_height;
            double crownRatio = lcp.bulk_density; //TODO: is this correct?

            //C#
            TwoFuelModelsMethod twoFuelModelsMethod = TwoFuelModelsMethod.NoMethod;
            BehaveUnits.MoistureUnits.MoistureUnitsEnum moistureUnits = BehaveUnits.MoistureUnits.MoistureUnitsEnum.Percent;
            WindHeightInputMode windHeightInputMode = WindHeightInputMode.DirectMidflame;
            BehaveUnits.SlopeUnits.SlopeUnitsEnum slopeUnits = BehaveUnits.SlopeUnits.SlopeUnitsEnum.Degrees;
            BehaveUnits.CoverUnits.CoverUnitsEnum coverUnits = BehaveUnits.CoverUnits.CoverUnitsEnum.Fraction;
            BehaveUnits.LengthUnits.LengthUnitsEnum lengthUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
            BehaveUnits.SpeedUnits.SpeedUnitsEnum windSpeedUnits = BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond;
            WindAndSpreadOrientationMode windAndSpreadOrientationMode = WindAndSpreadOrientationMode.RelativeToNorth;
            BehaveUnits.LengthUnits.LengthUnitsEnum canopyHeightUnits = BehaveUnits.LengthUnits.LengthUnitsEnum.Meters;
            BehaveUnits.DensityUnits.DensityUnitsEnum densityUnits = BehaveUnits.DensityUnits.DensityUnitsEnum.KilogramsPerCubicMeter;


            //feed new data
            //updates bith surface and crown
            fireMesh.crownFire.updateCrownInputs(fuelModelNumber,
                moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureFoliar, moistureUnits,
                windSpeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode,
                slope, slopeUnits, aspect,
                canopyCover, coverUnits, canopyHeight, canopyBaseHeight, canopyHeightUnits, crownRatio, canopyBulkDensity, densityUnits);

            //only updates surface, do not use
            /*fireMesh.crownFire.updateCrownsSurfaceInputs(fuelModelNumber, 
                moistureOneHour, moistureTenHour, moistureHundredHour, moistureLiveHerbaceous, moistureLiveWoody, moistureUnits, 
                windSpeed, windSpeedUnits, windHeightInputMode, windDirection, windAndSpreadOrientationMode, 
                slope, slopeUnits, aspect, 
                canopyCover, coverUnits, canopyHeight, canopyHeightUnits, crownRatio);*/

            //do new calc, do not use doCrownRunRothermel() as it seems buggy sine it doesn't save all sirface parameters as well as overwrites it with crown fire run
            fireMesh.crownFire.doCrownRunScottAndReinhardt();
            double currentMaxSpreadRate = fireMesh.crownFire.getFinalSpreadRate(BehaveUnits.SpeedUnits.SpeedUnitsEnum.MetersPerSecond);
            //bran-jnw: these output has been added by me in Crown.cs
            double directionOfMaxSpread = fireMesh.crownFire.getSurfaceFireDirectionOfMaxSpreadRate();
            double eccentricity_ = fireMesh.crownFire.getSurfaceFireEccentricity();

            if (currentMaxSpreadRate > maxSpreadRate)
            {
                maxSpreadRate = currentMaxSpreadRate;
                maxSpreadRateDirection = directionOfMaxSpread;
            }

            fireLineIntensity = fireMesh.crownFire.getFinalFirelineIntesity(BehaveUnits.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilojoulesPerMeterPerSecond);
            if (fireLineIntensity > maxFireLineIntensity)
            {
                maxFireLineIntensity = fireLineIntensity;
            }
            heatPerUnitArea = fireMesh.crownFire.getFinalHeatPerUnitArea() * 11.356538527057; //comes in btu/sq.feet, now in kJ/m2

            //we have the max spread rate, but need in all directions       
            for (int i = 0; i < spreadRates.Length; i++)
            {
                //not spreading in this direction
                if (neighbors[i] == null)
                {
                    continue;
                }
                //set default
                spreadRates[i] = currentMaxSpreadRate;

                //TODO: use this instead? have to get units and directions right...
                //spreadRates[i] = fireMesh.surfaceFire.calculateSpreadRateAtVector(directionOfInterest); 

                //TODO: canreplaced by calculateSpreadRateAtVector()
                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                // Calcualte beta: the angle between the direction of max spread and the direction of interest
                double directionOfInterest = RadToDegrees(fireMesh.GetAngleOffset(i));
                double beta = abs(directionOfMaxSpread - directionOfInterest);

                // Calculate the fire spread rate in this azimuth
                // if it deviates more than a tenth degree from the maximum azimuth
                if (beta > 180.0)
                {
                    beta = (360.0 - beta);
                }
                if (fabs(beta) > 0.1)
                {
                    double radians = beta * M_PI / 180.0;
                    spreadRates[i] = currentMaxSpreadRate * (1.0 - eccentricity_) / (1.0 - eccentricity_ * cos(radians));// * Random.Range(0.8f, 1.2f);                    
                }

                if (spreadRates[i] > maxSpreadRates[i])
                {
                    maxSpreadRates[i] = spreadRates[i];
                }
            }
        }

        //old, using BEHAVE instead, did not calculate corredt angle due to slope anyway
        /*public void UpdateSpreadRates()
        {
            //TODO: get values from BEHHAVE instead

            //the same as found in paper and fsxwmech.cpp, Meachanix::ellipse
            //length to breadth ratio
            double LB = 0.936 * exp(0.2566 * fireMesh.windSpeed) + 0.461 * exp(-0.1548 * fireMesh.windSpeed) - 0.397;
            // maximum "eccentricity"
            if (LB > 8.0)
            {
                LB = 8.0;
            }
            //head to back ratio
            double HB = (LB + sqrt(pow2(LB) - 1)) / (LB - sqrt(pow2(LB) - 1));
            double R = maxSpreadRate;
            double a = 0.5 * (R + R / HB) / LB;
            double b = 0.5 * (R + R / HB);
            //double c = b - R / HB;

            double f2 = pow2(a);
            double h2 = pow2(b);

            //http://mathworld.wolfram.com/Ellipse.html, https://en.wikipedia.org/wiki/Ellipse
            //use polar form relative to focus, find radius for each of the direction (N, NE, E...) which is the intersection with the ellipse along that direction
            //note: a and b are swapped compared to Farsite naming
            double e = sqrt(1 - f2 / h2);
            for (int i = 0; i < fireFront.Length; i++)
            {
                //not spreading in this direction
                if (fireFront[i] < 0)
                {
                    continue;
                }
                double o = fireMesh.GetAngleOffset(i);
                double r = b * (1 - pow2(e)) / (1 + e * cos(fireMesh.phi - o));
                spreadRates[i] = r;
            }
        }*/

        private void UpdateFireFront()
        {   
            for (int i = 0; i < fireFront.Length; i++)
            {
                //not spreading in this direction
                if (fireFront[i] < 0.0)
                {
                    continue;
                }  
                fireFront[i] += spreadRates[i] * fireMesh.dt;
            }        
        }

        public void CheckFireSpread()
        {
            if(cellState != FireCellState.Burning)
            {
                return;
            }

            int notDoneCount = 0;
            for (int i = 0; i < fireFront.Length; i++)
            {
                if(neighbors[i] == null) //fireFront[i] < 0.0
                {
                    continue;
                }
                //int xI = cellIndex.x + fireMesh.neighborIndices[i].x;
                //int yI = cellIndex.y + fireMesh.neighborIndices[i].y;

                WUInityFireCell f = neighbors[i];
                if (f.cellState == FireCellState.CanBurn)
                {
                    if (fireFront[i] >= distances[i])
                    {
                        double spill = fireFront[i] - distances[i];
                        f.QueueIgnition(i, spill);
                        fireFront[i] = -1.0;
                    }
                }
                else
                {
                    fireFront[i] = -1.0;
                }

                /*if (fireMesh.IsInsideMesh(xI, yI))
                {
                    WUInityFireCell f = fireMesh.GetFireCell(xI, yI);
                    if (f.cellState == FireCellState.CanBurn)
                    {                       
                        if (fireFront[i] >= distances[i])
                        {
                            double spill = fireFront[i] - distances[i];
                            f.QueueIgnition(i, spill);
                            fireFront[i] = -1;
                        }
                    }
                    else
                    {
                        fireFront[i] = -1;
                    }
                }
                else
                {
                    fireFront[i] = -1;
                }*/

                if (fireFront[i] >= 0.0)
                {
                    ++notDoneCount;
                }
            }
            if (notDoneCount == 0)
            {
                cellState = FireCellState.Dead;
                fireMesh.RemoveDeadCell(this);
            }            
        }

        public void CheckIfDead()
        {
            if (cellState != FireCellState.Burning)
            {
                return;
            }

            int notDoneCount = 0;
            for (int i = 0; i < fireFront.Length; i++)
            {
                if (neighbors[i] == null) //fireFront[i] < 0.0
                {
                    continue;
                }

                WUInityFireCell f = neighbors[i];
                if (f.cellState != FireCellState.CanBurn)
                {
                    fireFront[i] = -1.0;
                }
                /*int xI = cellIndex.x + fireMesh.neighborIndices[i].x;
                int yI = cellIndex.y + fireMesh.neighborIndices[i].y;
                if (fireMesh.IsInsideMesh(xI, yI))
                {
                    WUInityFireCell f = fireMesh.GetFireCell(xI, yI);
                    if (f.cellState != FireCellState.CanBurn)
                    {
                        fireFront[i] = -1.0;
                    }
                }
                else
                {
                    fireFront[i] = -1.0;
                }*/

                if (fireFront[i] >= 0.0)
                {
                    ++notDoneCount;
                }
            }
            if (notDoneCount == 0)
            {
                cellState = FireCellState.Dead;
                fireMesh.RemoveDeadCell(this);
            }
        }
    }
}

