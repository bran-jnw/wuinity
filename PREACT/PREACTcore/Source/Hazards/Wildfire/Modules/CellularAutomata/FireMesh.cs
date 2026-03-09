//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
using PREACT.Wildfire.Behave;
using PREACT.IO;
using PREACT.Math;
using System.Collections.Generic;

namespace PREACT.Wildfire
{
    [System.Serializable]                                           
    public class FireMesh : WildfireModule                        
    {
        Vector2int _cellCount;                                
        public FireCellInput.SpreadModes spreadMode;                               
        public List<IgnitionPointInput> ignitionPoints;                
        FireCell[] _fireCells;                                
        public Vector2d _cellSize;
        float cellArea;

        double _windSpeed, _windDirection;
        
        public double dt;                                           
        double[] angleOffsets;                                      
        public HashSet<FireCell> activeCells;                
        public HashSet<FireCell> cellsToKill;                
        public HashSet<FireCell> cellsToIgnite; 

        float[] fireLineIntensityData;
        float[] sootProduction;
        float[] fuelModelNumberData;

        public Vector2int[] neighborIndices;                        
        public double cellSizeDiagonal;                             
        public double cellSizeSquared;
        public double cellSizeDiagonalSquared;
        public double sixteenDist;
        public double sixteenDistSquared;
        public int indexSize;                                       

        private WeatherManager _weather;                                          
        public InitialFuelMoistureLibrary initialFuelMoisture;         
        
        public LandscapeData lcpData;                                     
        public FuelModelSet fuelModelSet;                           
        public Surface surfaceFire;                                 
        public Crown crownFire;

        private double timeSinceStart = 0.0;                        


        /*public FireMesh(string lcpFilename, WeatherInput weather, WindInput wind, InitialFuelMoistureList initialFuelMoisture, IgnitionPoint[] ignitionPoints)         
        {
            lcpData = new LCPData(lcpFilename);                 //import LCP data
            //create empry if we cannot read properly
            if(lcpData.CantAllocLCP)        
            {
                cellSize = new Vector2d(30, 30);                
            }
            else
            {
                cellSize = new Vector2d(lcpData.RasterCellResolutionX, lcpData.RasterCellResolutionY);            
            }
            cellCount = new Vector2int(Mathf.CeilToInt((float)(WUIEngine.Input.Simulation.Size.x / cellSize.x)), Mathf.CeilToInt((float)(WUIEngine.Input.Simulation.Size.y / cellSize.x)));        

            this.weather = weather;
            this.wind = wind;
            this.initialFuelMoisture = initialFuelMoisture;

            this.ignitionPoints = ignitionPoints;
        }*/

        public FireMesh(Simulation simulation, LandscapeData lcpData, WeatherManager weather, InitialFuelMoistureLibrary initialFuelMoisture, List<IgnitionPointInput> ignitionPoints) : base(simulation)        
        {

            this.lcpData = lcpData;
            _cellSize = new Vector2d(lcpData.RasterCellResolutionX, lcpData.RasterCellResolutionY);
            int xCells = lcpData.GetCellCountX();
            int yCells = lcpData.GetCellCountY();
            _cellCount = new Vector2int(xCells, yCells); //Vector2int(lcpData.Header.numeast, lcpData.Header.numnorth);           

            this._weather = weather;
            this.initialFuelMoisture = initialFuelMoisture;

            this.ignitionPoints = ignitionPoints;

            spreadMode = _simulation.Input.WildfireModule.FireCellInput.SpreadMode;

            InitializeMesh();
        }

        void InitializeMesh()                                                       
        {
            fuelModelSet = new FuelModelSet();
            //set custom fuel models if present
            if (_simulation.Input.WildfireModule.Data.FuelModelsData != null)
            {
                Engine.Message(null, Engine.LogType.Log, " Adding custom fuel model specifications.");
                for (int i = 0; i < _simulation.Input.WildfireModule.Data.FuelModelsData.Fuels.Count; i++)
                {
                    fuelModelSet.setFuelModelRecord(_simulation.Input.WildfireModule.Data.FuelModelsData.Fuels[i]);
                }
            }                    
            surfaceFire = new Surface(fuelModelSet);            
            crownFire = new Crown(fuelModelSet);                         

            indexSize = 4;                                      
            if (spreadMode == FireCellInput.SpreadModes.EightDirections)
            {
                indexSize = 8;
            }
            else if (spreadMode == FireCellInput.SpreadModes.SixteenDirections)
            {
                indexSize = 16;
            }
            angleOffsets = new double[indexSize];                   
            neighborIndices = new Vector2int[indexSize];            
            neighborIndices[0] = Vector2int.up;
            neighborIndices[1] = Vector2int.right;
            neighborIndices[2] = Vector2int.down;
            neighborIndices[3] = Vector2int.left;
            if (indexSize >= 8)
            {
                neighborIndices[4] = Vector2int.up + Vector2int.right;
                neighborIndices[5] = Vector2int.right + Vector2int.down;
                neighborIndices[6] = Vector2int.down + Vector2int.left;
                neighborIndices[7] = Vector2int.left + Vector2int.up;
            }
            if (indexSize >= 16)
            {
                neighborIndices[8] = 2 * Vector2int.up + Vector2int.right;
                neighborIndices[9] = Vector2int.up + 2 * Vector2int.right;
                neighborIndices[10] = 2 * Vector2int.right + Vector2int.down;
                neighborIndices[11] = Vector2int.right + 2 * Vector2int.down;
                neighborIndices[12] = 2 * Vector2int.down + Vector2int.left;
                neighborIndices[13] = Vector2int.down + 2 * Vector2int.left;
                neighborIndices[14] = 2 * Vector2int.left + Vector2int.up;
                neighborIndices[15] = Vector2int.left + 2 * Vector2int.up;
            }
            for (int i = 0; i < indexSize; ++i)                                 
            {
                if (i < 4)
                {
                    angleOffsets[i] = i * 0.5 * Mathd.PI;// + Mathd.PI; //Mathd.PI added since wind is opposite direction it blows. CORRECTION: should be correct without
                }
                else if (i < 8)
                {
                    angleOffsets[i] = (i - 4) * 0.5 * Mathd.PI + 0.25 * Mathd.PI;// + Mathd.PI;
                }
                else
                {
                    angleOffsets[i] = (i - 8) * 0.25 * Mathd.PI + 0.125 * Mathd.PI;// + Mathd.PI;
                }

            }
            cellSizeDiagonal = _cellSize.x * Mathd.Sqrt(2.0);                     
            cellSizeSquared = _cellSize.x * _cellSize.x;
            cellSizeDiagonalSquared = cellSizeDiagonal * cellSizeDiagonal;
            sixteenDist = Mathd.Sqrt(5.0) * _cellSize.x;
            sixteenDistSquared = sixteenDist * sixteenDist;

            _fireCells = new FireCell[_cellCount.x * _cellCount.y];         
            for (int y = 0; y < _cellCount.y; ++y)                                      
            {
                for (int x = 0; x < _cellCount.x; ++x)
                {                    
                    LandscapeCellData l = lcpData.GetCellDataSimulationIndex(x, y, true);                      
                    _fireCells[GetCellIndex(x, y)] = new FireCell(this, x, y, l); 
                }
            }

            //calc distances based on elevation etc
            for (int y = 0; y < _cellCount.y; ++y)
            {
                for (int x = 0; x < _cellCount.x; ++x)
                {
                    //send data to cell, fuel type etc
                    _fireCells[GetCellIndex(x, y)].InitCell();                           
                }
            }
            activeCells = new HashSet<FireCell>();                               
            cellsToKill = new HashSet<FireCell>();
            cellsToIgnite = new HashSet<FireCell>();            

            /*for (int y = 0; y < cellCount.y; y++)
            {
                for (int x = 0; x < cellCount.x; x++)
                {
                    float tint = fireCells[GetCellIndex(x, y)].GetFuelModelNumber() / 13.0f;
                    Color c = Color.green * tint;
                    c.a = 0.4f;                  
                }
            }*/

            //data arrays for visualization
            fireLineIntensityData = new float[_fireCells.Length];
            fuelModelNumberData = new float[_fireCells.Length];
            for (int i = 0; i < _fireCells.Length; i++)
            {
                fuelModelNumberData[i] = _fireCells[i].GetFuelModelNumber();
            }
            sootProduction = new float[_fireCells.Length];
            cellArea = (float)(_cellSize.x * _cellSize.y);

            UpdateIgnitionPoints(0.0f);
            UpdateCellSpreadRates();

            _initialized = true;
        }

        double GetCorrectedElevation(int x, int y)                  
        {            
            return _fireCells[GetCellIndex(x, y)].GetElevation() - lcpData.GetElevationMinMax().x;           
        }

        double GetRawElevation(int x, int y)                        
        {
            return _fireCells[GetCellIndex(x, y)].GetElevation();
        }

        public void AddCellToIgnite(FireCell f)              
        {
            if(!cellsToIgnite.Contains(f))
            {                
                cellsToIgnite.Add(f);
            }            
        }

        public void RemoveDeadCell(FireCell f)               
        {
            cellsToKill.Add(f);            
        }

        public double GetAngleOffset(int i)                        
        {
            return angleOffsets[i];
        }

        bool _initialized = false;
        List<Vector2int> _ignitedCells = new List<Vector2int>();
        public override void Step(float currentTime, float deltaTime)
        {            
            UpdateIgnitionPoints((float)timeSinceStart);

            if (activeCells.Count == 0)                             
            {
                return;// false;
            }            

            //collect max spread rate
            double maxSpreadRate = -1.0;
            double maxFireLineIntensity = -1.0;
            foreach (FireCell f in activeCells)
            {
                double sR = f.GetMaxSpreadRate();
                if (sR > maxSpreadRate)
                {
                    maxSpreadRate = sR;
                }
                double fLI = f.GetFireLineIntensity(true);
                if (fLI > maxFireLineIntensity)
                {
                    maxFireLineIntensity = fLI;
                }
            }
            
            //calculate max dt
            dt = _cellSize.x / maxSpreadRate;        

            //move fire fronts
            foreach (FireCell f in activeCells)
            {
                f.Burn();
            }

            //check if spread occurs, tags cells that gets ignited, "add" spill
            foreach (FireCell f in activeCells)
            {
                f.CheckFireSpread();
            }
                        
            //remove dead cells (that has nowhere to spread according to CheckFireSpread)
            foreach (FireCell f in cellsToKill)
            {
                activeCells.Remove(f);
            }
            cellsToKill.Clear();

            //add the ones that were ignited to the active list
            foreach (FireCell f in cellsToIgnite)
            {
                _ignitedCells.Add(f.cellIndex);
                f.Ignite(timeSinceStart);
                activeCells.Add(f);
            }
            cellsToIgnite.Clear();

            //second round of checking if dead since some neighbors might have changed status after kill & ignite loops
            foreach (FireCell f in activeCells)
            {
                f.CheckIfDead();
            }

            //remove dead cells (that has nowhere to spread)
            foreach (FireCell f in cellsToKill)
            {
                activeCells.Remove(f);
            }

            //update data arrays for visualization and input for smoke spread if needed
            float dtInversed = 1.0f / (float)dt;
            for (int i = 0; i < _fireCells.Length; i++)
            {
                fireLineIntensityData[i] = (float)_fireCells[i].GetFireLineIntensity(false);
                if(_simulation.Input.SmokeModule.Enabled)
                {
                    sootProduction[i] = 0.0f;
                    if (_fireCells[i].cellState == FireCellState.Burning)
                    {
                        //[kg/s], intensity is kW/m2, assume 8000 btu/lb is 18608 kJ/kg HOC, soot yield 0.015 for wood found for FDS
                        sootProduction[i] = Mathf.Max(0.0f, 0.015f * (float)_fireCells[i].GetTimestepBurntMass() * dtInversed);
                    }
                }                
            }

            //update time and wind for next time step. TODO: spread out the update over several frames
            timeSinceStart += dt;
            _weather.GetWind(out _windSpeed, out _windDirection);
            //TODO: only update if any input has changed, re-calculate spread rates
            UpdateCellSpreadRates();

            //return true;
        }

        public override bool IsSimulationDone()
        {
            if(activeCells.Count == 0 && _ignitionDone)
            {
                return true;
            }

            return false;
        }

        public override int GetActiveCellCount()
        {
            return activeCells.Count;
        }

        void UpdateCellSpreadRates()
        {
            foreach (FireCell f in activeCells)
            {                
                f.UpdateSpreadRates();
            }
        }

        int activatedIgnitions = 0;
        bool _ignitionDone = false;
        public void UpdateIgnitionPoints(float currentTime)
        {
            if (_ignitionDone)
            {
                return;
            }

            if(_simulation.Input.WildfireModule.FireCellInput.UseInitialIgnitionMap)
            {
                for (int i = 0; i < _fireCells.Length; i++)
                {
                    if (_simulation.Input.WildfireModule.Data.InitialIgnition[i])
                    {
                        FireCell f = _fireCells[i];
                        f.Ignite(currentTime);
                        //we might try to ignite on a cell that is dead which will then not be intiialized correctly
                        if (f.cellState != FireCellState.Dead)
                        {
                            activeCells.Add(f);
                        }                        
                    }
                }

                _ignitionDone = true;
            }
            else
            {
                for (int i = 0; i < ignitionPoints.Count; ++i)
                {
                    /*if (!ignitionPoints[i].HasBeenIgnited() && ignitionPoints[i].IgnitionTime <= currentTime)
                    {
                        ignitionPoints[i].CalculateMeshIndex(_simulation, this);
                        if (ignitionPoints[i].IsInsideFire(_cellCount))
                        {
                            int x = ignitionPoints[i].GetX();
                            int y = ignitionPoints[i].GetY();
                            FireCell f = _fireCells[GetCellIndex(x, y)];
                            f.Ignite(currentTime);
                            activeCells.Add(f);
                            ignitionPoints[i].MarkAsIgnited();

                            Engine.Message(null, Engine.LogType.Log, " Ignition started in cell " + x + ", " + y + " which has fuel model number " + f.GetFuelModelNumber());
                        }
                        ++activatedIgnitions;
                    }*/
                }

                if (activatedIgnitions == ignitionPoints.Count)
                {
                    _ignitionDone = true;
                }
            }            
        }

        public int GetCellIndex(int x, int y)
        {
            //too slow...
            //clamp
            //x = Mathf.Clamp(x, 0, cellCount.x - 1);                 
            //y = Mathf.Clamp(y, 0, cellCount.y - 1);
            
            return (x + y * _cellCount.x);
        }

        public bool IsInsideMesh(int x, int y)                      
        {
            if (x < 0 || x > _cellCount.x - 1 || y < 0 || y > _cellCount.y - 1)
            {
                return false;
            }
            return true;
        }

        public FireCell GetFireCell(int index)
        {
            return _fireCells[index];
        }

        public FireCell GetFireCell(int x, int y)            
        {
            return _fireCells[GetCellIndex(x, y)];
        }

        public override float[] GetSootProduction()
        {
            return sootProduction;
        }

        public override FireCellState GetFireCellState(Vector2d latLong)     
        {
            Vector2d pos = _simulation.GetSimulationPosition(latLong);

            int x = (int)(pos.x / _cellSize.x);
            int y = (int)(pos.y / _cellSize.x);

            //might be called before initialized, so need to check null, but should really change execution order
            if(!IsInsideMesh(x, y) || _fireCells == null)
            {
                return FireCellState.Dead;
            }
            else
            {
                return _fireCells[GetCellIndex(x, y)].cellState;
            }            
        }     
                
        public override float[] GetFireLineIntensityData()
        {     
            return fireLineIntensityData;
        }

        public override float[] GetFuelModelNumberData()
        {
            return fuelModelNumberData;
        }

        public int GetCellSize()
        {
            return (int)_cellSize.x;
        }

        public Vector2int GetCellCount()
        {
            return _cellCount;
        }

        public override float[,] GetMaxROS()                                   
        {
            float[,] maxROS = new float[_cellCount.x, _cellCount.y];
            for (int j = 0; j < _cellCount.y; j++)
            {
                for (int i = 0; i < _cellCount.x; i++)
                {
                    //flip on y-axis
                    int yIndex = _cellCount.y - 1 - j;
                    FireCell f = _fireCells[GetCellIndex(i, yIndex)];
                    maxROS[i, j] = (float)f.GetMaxSpreadRate();

                    /*for (int k = 0; k < 8; k++)
                    {
                        ros[j + i * _cellCount.y, k] = f.GetMaxSpreadrateInDirection(k);
                    }*/
                }
            }
            return maxROS;
        }

        public override float[,] GetMaxROSAzimuth()
        {
            float[,] maxROSAzimuth = new float[_cellCount.x, _cellCount.y];
            for (int j = 0; j < _cellCount.y; j++)
            {
                for (int i = 0; i < _cellCount.x; i++)
                {
                    //flip on y-axis
                    int yIndex = _cellCount.y - 1 - j;
                    FireCell f = _fireCells[GetCellIndex(i, yIndex)];
                    maxROSAzimuth[i, j] = (float)f.GetMaxSpreadRateAzimuth();
                }
            }
            return maxROSAzimuth;
        }

        public override double GetInternalDeltaTime()
        {
            return dt;
        }

        public override int GetCellCountX()
        {
            return _cellCount.x;
        }

        public override int GetCellCountY()
        {
            return _cellCount.x;
        }

        public override float GetCellSizeX()
        {
            return (float)_cellSize.x;
        }

        public override float GetCellSizeY()
        {
            return (float)_cellSize.y;
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            return _ignitedCells;
        }

        public override void ConsumeIgnitedFireCells()
        {
            _ignitedCells.Clear();
        }

        public override void Stop()
        {
            //nothing to do
        }

        public override void GetOffsetAndSize(out Vector2d offset, out Vector2d size)
        {
            offset = _originOffset;
            size = new Vector2d(lcpData.GetLandscapeSizeX(), lcpData.GetLandscapeSizeY());
        }
    }
}
