using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System;

namespace WUIEngine.Fire
{
    [System.Serializable]                                           
    public class FireMesh : FireModule                        
    {
        Vector2int _cellCount;                                
        public SpreadMode spreadMode;                               
        public IgnitionPoint[] ignitionPoints;                
        FireCell[] _fireCells;                                
        public Vector2d _cellSize;
        float cellArea;
        WindData _currentWindData;                            
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

        private WeatherInput weather;                               
        private WindInput wind;                                     
        public InitialFuelMoistureList initialFuelMoisture;         
        
        public LCPData lcpData;                                     
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

        public FireMesh(LCPData lcpData, WeatherInput weather, WindInput wind, InitialFuelMoistureList initialFuelMoisture, IgnitionPoint[] ignitionPoints)        
        {
            this.lcpData = lcpData;
            _cellSize = new Vector2d(lcpData.RasterCellResolutionX, lcpData.RasterCellResolutionY);
            int xCells = (int)(Engine.INPUT.Simulation.Size.x / _cellSize.x);
            int yCells = (int)(Engine.INPUT.Simulation.Size.y / _cellSize.y);
            _cellCount = new Vector2int(xCells, yCells); //Vector2int(lcpData.Header.numeast, lcpData.Header.numnorth);           

            this.weather = weather;
            this.wind = wind;
            this.initialFuelMoisture = initialFuelMoisture;

            this.ignitionPoints = ignitionPoints;

            spreadMode = Engine.INPUT.Fire.spreadMode;

            InitializeMesh();
        }

        void InitializeMesh()                                                       
        {
            fuelModelSet = new FuelModelSet();
            //set custom fuel models if present
            if(Engine.DATA_STATUS.FuelModelsLoaded)
            {
                Engine.LOG(Engine.LogType.Log, " Adding custom fuel model specifications.");
                for (int i = 0; i < Engine.RUNTIME_DATA.Fire.FuelModelsData.Fuels.Count; i++)
                {
                    fuelModelSet.setFuelModelRecord(Engine.RUNTIME_DATA.Fire.FuelModelsData.Fuels[i]);
                }
            }            
            surfaceFire = new Surface(fuelModelSet);            
            crownFire = new Crown(fuelModelSet);                         

            indexSize = 4;                                      
            if (spreadMode == SpreadMode.EightDirections)
            {
                indexSize = 8;
            }
            else if (spreadMode == SpreadMode.SixteenDirections)
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
                    angleOffsets[i] = i * 0.5 * Math.PI;// + Math.PI; //Math.PI added since wind is opposite direction it blows. CORRECTION: should be correct without
                }
                else if (i < 8)
                {
                    angleOffsets[i] = (i - 4) * 0.5 * Math.PI + 0.25 * Math.PI;// + Math.PI;
                }
                else
                {
                    angleOffsets[i] = (i - 8) * 0.25 * Math.PI + 0.125 * Math.PI;// + Math.PI;
                }

            }
            cellSizeDiagonal = _cellSize.x * Math.Sqrt(2.0);                     
            cellSizeSquared = _cellSize.x * _cellSize.x;
            cellSizeDiagonalSquared = cellSizeDiagonal * cellSizeDiagonal;
            sixteenDist = Math.Sqrt(5.0) * _cellSize.x;
            sixteenDistSquared = sixteenDist * sixteenDist;

            _fireCells = new FireCell[_cellCount.x * _cellCount.y];         
            for (int y = 0; y < _cellCount.y; ++y)                                      
            {
                for (int x = 0; x < _cellCount.x; ++x)
                {                    
                    LandScapeStruct l = lcpData.GetCellData(x, y, true);                      
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

            initialized = true;
        }

        double GetCorrectedElevation(int x, int y)                  
        {            
            return _fireCells[GetCellIndex(x, y)].GetElevation() - lcpData.Header.loelev;           
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

        bool initialized = false;
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
                if(Engine.INPUT.Simulation.RunSmokeModule)
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
            _currentWindData = wind.GetWindDataAtTime((float)timeSinceStart);
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

            if(Engine.INPUT.Fire.useInitialIgnitionMap)
            {
                for (int i = 0; i < _fireCells.Length; i++)
                {
                    if (Engine.RUNTIME_DATA.Fire.InitialIgnitionIndices[i])
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
                for (int i = 0; i < ignitionPoints.Length; ++i)
                {
                    if (!ignitionPoints[i].HasBeenIgnited() && ignitionPoints[i].IgnitionTime <= currentTime)
                    {
                        ignitionPoints[i].CalculateMeshIndex(this);
                        if (ignitionPoints[i].IsInsideFire(_cellCount))
                        {
                            int x = ignitionPoints[i].GetX();
                            int y = ignitionPoints[i].GetY();
                            FireCell f = _fireCells[GetCellIndex(x, y)];
                            f.Ignite(currentTime);
                            activeCells.Add(f);
                            ignitionPoints[i].MarkAsIgnited();

                            Engine.LOG(Engine.LogType.Log, " Ignition started in cell " + x + ", " + y + " which has fuel model number " + f.GetFuelModelNumber());
                        }
                        ++activatedIgnitions;
                    }
                }

                if (activatedIgnitions == ignitionPoints.Length)
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
            Vector2d pos = Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.WUInity.MAP.CenterMercator, WUInity.WUInity.MAP.WorldRelativeScale);

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

        public override int[,] GetMaxROS()                                   
        {
            int[,] ros = new int[_cellCount.x * _cellCount.y, 8];
            for (int i = 0; i < _cellCount.x; i++)
            {
                for (int j = 0; j < _cellCount.y; j++)
                {
                    //flip on y-axis
                    int yIndex = _cellCount.y - 1 - j;
                    FireCell f = _fireCells[GetCellIndex(i, yIndex)];
                    for (int k = 0; k < 8; k++)
                    {
                        ros[j + i * _cellCount.y, k] = f.GetMaxSpreadrateInDirection(k);
                    }
                }
            }
            return ros;
        }

        public override float GetInternalDeltaTime()
        {
            return (float)dt;
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

        public override WindData GetCurrentWindData()
        {
            return _currentWindData;
        }

        public override List<Vector2int> GetIgnitedFireCells()
        {
            throw new NotImplementedException();
        }

        public override void ConsumeIgnitedFireCells()
        {
            throw new NotImplementedException();
        }

        //remove? as we now visualize using fire renderer
        /*void CreateTerrainPlane()
        {
            terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            terrainMesh.Clear();
            CreatePlane(terrainMesh, cellCount.x, cellCount.y, (float)(cellCount.x * cellSize.x), (float)(cellCount.y * cellSize.y), 0.0f, Vector2.one, true);
        }*/

        /*private void CreatePlane(Mesh mesh, int cellsX, int cellsZ, float sizeX, float sizeZ, float yPos, Vector2 maxUV, bool getElevation)
        {
            //add one since we always need one extra, e.g..1 cell equals 4 vertices
            cellsX += 1;
            cellsZ += 1;

            // ignore resolution for now
            cellsX = 2;
            cellsZ = 2;                         
            getElevation = false;

            Vector3[] vertices = new Vector3[cellsX * cellsZ];              
            for (int z = 0; z < cellsZ; z++)
            {
                float zPos = ((float)z / (cellsZ - 1)) * sizeZ;
                for (int x = 0; x < cellsX; x++)
                {
                    float xPos = ((float)x / (cellsX - 1)) * sizeX;
                    if (getElevation)
                    {
                        int xIndex = Mathf.RoundToInt(cellCount.x * (float)x / (cellsX - 1));
                        int yIndex = Mathf.RoundToInt(cellCount.y * (float)z / (cellsZ - 1));
                        yPos = (float)GetCorrectedElevation(xIndex, yIndex);
                    }
                    vertices[x + z * cellsX] = new Vector3(xPos, yPos, zPos);
                }
            }
            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < normals.Length; n++)
            {
                normals[n] = Vector3.up;
            }
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int v = 0; v < cellsZ; v++)
            {
                for (int u = 0; u < cellsX; u++)
                {
                    uvs[u + v * cellsX] = new Vector2((float)u / (cellsX - 1) * maxUV.x, (float)v / (cellsZ - 1) * maxUV.y);
                }
            }
            int nbFaces = (cellsX - 1) * (cellsZ - 1);
            int[] triangles = new int[nbFaces * 6];
            int index = 0;
            for (int y = 0; y < cellsZ - 1; y++)
            {
                for (int x = 0; x < cellsX - 1; x++)
                {
                    triangles[index] = (y * cellsX) + x;
                    triangles[index + 1] = ((y + 1) * cellsX) + x;
                    triangles[index + 2] = (y * cellsX) + x + 1;

                    triangles[index + 3] = ((y + 1) * cellsX) + x;
                    triangles[index + 4] = ((y + 1) * cellsX) + x + 1;
                    triangles[index + 5] = (y * cellsX) + x + 1;
                    index += 6;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            //mesh.RecalculateTangents();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }*/
    }
}
