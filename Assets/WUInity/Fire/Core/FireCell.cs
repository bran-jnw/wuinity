using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.Fire.MathWrap;
using System.Runtime.InteropServices;
using UnityEditor;

namespace WUInity.Fire
{  
    //[System.Serializable]
    public class FireCell                
    {       
        public Vector2Int cellIndex;            //Declare a cellIndex vector   
        public FireCellState cellState;         //Declare a variable of type FireCellState (where is that defined)

        //firefront is the different spread directions and how far they have travelled
        double[] fireFront; //gets set to -1 when done spreading, otherwise always positive
        //this is teh distance to the next cell center, so when firefront has passed distance whe have spread
        double[] distances;
        double[] spreadRates;
        FireCell[] neighbors;
        FireMesh fireMesh;
        LandScapeStruct lcp;
        double maxSpreadRate;
        double maxSpreadRateDirection;
        double[] maxSpreadRates;
        double reactionIntensity;
        double fireLineIntensity;
        double maxFireLineIntensity;
        double timeOfArrival;
        double heatPerUnitArea;
        double currentBurnArea;
        double timestepBurntMass;
        double fuelMassGround;
        bool doneSpreading;
        InitialFuelMoisture moisture;

        [DllImport("BEHAVEDLL", EntryPoint = "CalcFireMaxSpreadRate")]          
        public static extern double CalcFireMaxSpreadRate(int fuelModelNumber, 
            double moistureOneHour, double moistureTenHour, double moistureHundredHour, double moistureLiveHerbaceous, double moistureLiveWoody,
            double windSpeed, double windDirection,
            double slope, double aspect,
            double directionOfInterest,
            double canopyCover, double canopyHeight, double crownRatio);
        

        public FireCell(FireMesh fireMesh, int xIndex, int yIndex, LandScapeStruct lcp)
        {
            this.fireMesh = fireMesh;
            cellIndex = new Vector2Int(xIndex, yIndex);
            cellState = FireCellState.CanBurn;                                  

            fireFront = new double[fireMesh.indexSize];
            distances = new double[fireMesh.indexSize];
            spreadRates = new double[fireMesh.indexSize];
            maxSpreadRates = new double[fireMesh.indexSize];
            neighbors = new FireCell[fireMesh.indexSize];

            this.lcp = lcp;       
        }

        /// <summary>
        /// Sets neightbors, calculates distance to neighbor cell centers and calculates slope and aspect
        /// </summary>
        public void InitCell()
        {
            currentBurnArea = 0.0;
            timestepBurntMass = 0.0;
            timeOfArrival = -9999.0;
            doneSpreading = false;

            int fuel = lcp.fuel_model;
            if (fireMesh.surfaceFire.isAllFuelLoadZero(fuel))
            {
                doneSpreading = true;
                cellState = FireCellState.Dead;
                return;
            }

            //calc distance to next cell center in current direction
            for (int i = 0; i < distances.Length; i++)                                  
            {
                int xI = cellIndex.x + fireMesh.neighborIndices[i].x;               
                int yI = cellIndex.y + fireMesh.neighborIndices[i].y;              
                if (fireMesh.IsInsideMesh(xI, yI) && fireMesh.GetFireCell(xI, yI).cellState == FireCellState.CanBurn)              
                {
                    FireCell neighborCell= fireMesh.GetFireCell(xI, yI);             
                    //set neighbor since it exists
                    neighbors[i] = neighborCell;                                               
                    
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
                    distances[i] = sqrt(2 * dist + pow2(lcp.elevation- neighborCell.lcp.elevation));           
                }
                else                                                           
                {
                    //UPDATE: set to 0 instead of -1 since we need spread to calculate current burn area
                    fireFront[i] = 0.0;
                    distances[i] = -1.0;  
                }
                maxSpreadRates[i] = 0.0;
            }

            //in case we are missing slope and aspect data for some reason we calculate them (e.g. if using heightmaps and not LCP)
            if(lcp.slope == -1 && lcp.aspect == -1)                                                 
            {
                CalculateSlopeAndAspect();
            }                      
            maxSpreadRate = -1.0;

            moisture = fireMesh.initialFuelMoisture.GetInitialFuelMoisture(lcp.fuel_model);

            double bulkDensity = fireMesh.fuelModelSet.getFuelLoadOneHour(fuel, BehaveUnits.LoadingUnits.LoadingUnitsEnum.KilogramsPerSquareMeter);
            bulkDensity += fireMesh.fuelModelSet.getFuelLoadTenHour(fuel, BehaveUnits.LoadingUnits.LoadingUnitsEnum.KilogramsPerSquareMeter);
            bulkDensity += fireMesh.fuelModelSet.getFuelLoadHundredHour(fuel, BehaveUnits.LoadingUnits.LoadingUnitsEnum.KilogramsPerSquareMeter);
            bulkDensity += fireMesh.fuelModelSet.getFuelLoadLiveHerbaceous(fuel, BehaveUnits.LoadingUnits.LoadingUnitsEnum.KilogramsPerSquareMeter);
            bulkDensity += fireMesh.fuelModelSet.getFuelLoadLiveWoody(fuel, BehaveUnits.LoadingUnits.LoadingUnitsEnum.KilogramsPerSquareMeter);
            fuelMassGround = bulkDensity * fireMesh.cellSize.x * fireMesh.cellSize.y;// * fireMesh.fuelModelSet.getFuelbedDepth(fuel, BehaveUnits.LengthUnits.LengthUnitsEnum.Meters);                     
        }

        public void Ignite(double timeOfArrival)            
        {
            //since this might get called by both ignition point AND from spreading from neighbors we have to check so we do not update it twice
            if(this.timeOfArrival < 0.0)
            {
                this.timeOfArrival = timeOfArrival;
            }   
            
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

        public double GetReactionIntensity()
        {
            return reactionIntensity;
        }

        public FireMesh GetMesh()
        {
            return fireMesh;
        }

        public short GetCanopyCover()
        {
            return lcp.canopy_cover;
        }

        public double GetCurrentBurnArea()
        {
            return currentBurnArea;
        }

        public double GetTimestepBurntMass()
        {
            return timestepBurntMass;
        }

        /// <summary>
        /// Direction zero is North, then goes clockwise, only 0-7 is valid.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public int GetMaxSpreadrateInDirection(int direction)
        {
            int rate = 0;
            if (fireMesh.spreadMode == SpreadMode.FourDirections)
            {
                //interpolate?
            }
            else
            {
                //north, east, south, west
                if (direction == 0 || direction % 2 != 0)                    //@Jonathan, this if condition returns true for direction = [0,1,3,5,7], did you mean mode2 == 0?
                {
                    direction = direction / 2;
                }
                //NE, SE, SW, NW
                else
                {
                    direction = direction / 2 + 4;
                }
                rate = (int)maxSpreadRates[direction];
            }
            return rate;
        }

        public void Burn()
        {
            if(cellState != FireCellState.Burning)
            {
                return;
            }

            UpdateFireFront();

            //consume mass, check is done in check fire spread
            double oldBurnArea = currentBurnArea;
            //the 0.5 factor comes from the prism that is created by the four cardinal fire fronts, as these will fully cover 2x2 cells when having spread fully we reduce the area by a factor of 0.5 to normalize to actual cell area
            currentBurnArea = 0.5 * (fireFront[0] + fireFront[2]) * (fireFront[1] + fireFront[3]);
            //TODO: limit area based on actual distance (slope correction)
            currentBurnArea = System.Math.Min(currentBurnArea, fireMesh.cellSize.x * fireMesh.cellSize.y);

            //burnt mass is based on half-point integral from old area and new area
            //timestepBurntMass = fireMesh.dt * (currentBurnArea - oldBurnArea) * 0.5 * reactionIntensity / 18608.0;
            //old way assumes everything keeps burning, not just flame front, this does not make sense however, instead add old area as smouldering as long as fuel mass is present
            timestepBurntMass = fireMesh.dt * ((currentBurnArea - oldBurnArea) * 0.5 + oldBurnArea) * reactionIntensity / 18608.0;  
            fuelMassGround -= timestepBurntMass;
            //MonoBehaviour.print("Burn area: " + currentBurnArea + ", delta mass: " + timestepBurntMass + ", fuel mass left: " + fuelMassGround);                                 
        }

        public double GetMaxSpreadRate()
        {
            return maxSpreadRate;
        }        

        private void UpdateFireFront()
        {   
            for (int i = 0; i < fireFront.Length; i++)
            {
                //not spreading in this direction
                //UPDATE: spread in all direction for current burn area calculation
                /*if (fireFront[i] < 0.0)
                {
                    continue;
                }*/
                fireFront[i] += spreadRates[i] * fireMesh.dt;
            }        
        }

        public void CheckFireSpread()
        {
            if(cellState != FireCellState.Burning)
            {
                return;
            }

            if(!doneSpreading)
            {
                int notDoneCount = 0;
                for (int i = 0; i < fireFront.Length; i++)
                {
                    if (neighbors[i] == null)
                    {
                        continue;
                    }

                    FireCell neighborCell = neighbors[i];
                    if (neighborCell.cellState == FireCellState.CanBurn)
                    {
                        if (fireFront[i] >= distances[i])
                        {
                            double spill = fireFront[i] - distances[i];
                            neighborCell.QueueIgnition(i, spill);
                        }
                        else
                        {
                            ++notDoneCount;
                        }
                    }
                }

                //we got nowhere to spread
                if (notDoneCount == 0)
                {
                    doneSpreading = true;
                }
            }            

            if (fuelMassGround <= 0.0 && doneSpreading)
            {
                cellState = FireCellState.Dead;
                fireMesh.RemoveDeadCell(this);
            }
        }

        public void CheckIfDead()
        {
            if (cellState != FireCellState.Burning || doneSpreading)
            {
                return;
            }

            int notDoneCount = 0;
            for (int i = 0; i < fireFront.Length; i++)
            {
                if (neighbors[i] == null)
                {
                    continue;
                }

                FireCell neighborCell = neighbors[i];
                if (neighborCell.cellState == FireCellState.CanBurn)
                {
                    ++notDoneCount;
                }
            }

            if (notDoneCount == 0)
            {
                doneSpreading = true;
                //this should only happen in one place, done in fire spread for now
                //cellState = FireCellState.Dead;
                //fireMesh.RemoveDeadCell(this);
            }
        }


        public void UpdateSpreadRates()
        {
            //replaces code below as it calls surface calc too
            /*UpdateCrownFireSpreadRates();
            return;*/
            
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
            double canopyHeight = lcp.crown_canopy_height;
            double crownRatio = lcp.crown_bulk_density; //TODO: is this correct?

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

            reactionIntensity = fireMesh.surfaceFire.getReactionIntensity(BehaveUnits.HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum.KilowattsPerSquareMeter);

            fireLineIntensity = fireMesh.surfaceFire.getFirelineIntensity(BehaveUnits.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter);
            if (fireLineIntensity > maxFireLineIntensity)
            {
                maxFireLineIntensity = fireLineIntensity;
            }
            heatPerUnitArea = fireMesh.surfaceFire.getHeatPerUnitArea() * 11.356538527057; //comes in btu/sq.feet, now in kJ/m2

            //we have the max spread rate, but need in all directions       
            for (int i = 0; i < spreadRates.Length; i++)
            {
                //not spreading in this direction, but we are always calculate the cardinal directions since we need to keep track of current cell burn area
                if (neighbors[i] == null && i > 3)
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
            InitialFuelMoisture moisture = fireMesh.initialFuelMoisture.GetInitialFuelMoisture(lcp.fuel_model);
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
            double canopyBaseHeight = lcp.crown_canopy_height;
            double canopyBulkDensity = lcp.crown_bulk_density;


            double windSpeed = fireMesh.currentWindData.speed;
            double windDirection = fireMesh.currentWindData.direction;
            // Wind adjustment factor parameters
            double canopyCover = lcp.canopy_cover;
            double canopyHeight = lcp.crown_canopy_height;
            double crownRatio = lcp.crown_bulk_density; //TODO: is this correct?

            //C#, i guess this sets up a lot of enumerators that set the units?
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
            //updates both surface and crown (shove it all in behave?)
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

            //do new calc, do not use doCrownRunRothermel() as it seems buggy since it doesn't save all surface parameters as well as overwrites it with crown fire run
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

            reactionIntensity = fireMesh.surfaceFire.getReactionIntensity(BehaveUnits.HeatSourceAndReactionIntensityUnits.HeatSourceAndReactionIntensityUnitsEnum.KilowattsPerSquareMeter);

            fireLineIntensity = fireMesh.crownFire.getFinalFirelineIntesity(BehaveUnits.FirelineIntensityUnits.FirelineIntensityUnitsEnum.KilowattsPerMeter);
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
    }
}

