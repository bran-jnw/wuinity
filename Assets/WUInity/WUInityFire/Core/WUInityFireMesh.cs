using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace WUInity.Fire
{
    [System.Serializable]
    public class WUInityFireMesh
    {
        public Vector2Int cellCount;
        public SpreadMode spreadMode;
        public WUInityFireIgnition[] ignitionPoints;
        WUInityFireCell[] fireCells;
        public Vector2D cellSize = new Vector2D(30.0, 30.0);
        public Texture2D burnTexture;        
        public WindData currentWindData;
        public double dt;
        double[] angleOffsets;
        public HashSet<WUInityFireCell> activeCells;
        public HashSet<WUInityFireCell> cellsToKill;
        public HashSet<WUInityFireCell> cellsToIgnite;
        public Mesh terrainMesh;

        public Vector2Int[] neighborIndices;
        public double cellSizeDiagonal;
        public double cellSizeSquared;
        public double cellSizeDiagonalSquared;
        public double sixteenDist;
        public double sixteenDistSquared;
        public int indexSize;

        private WeatherInput weather;
        private WindInput wind;
        public InitialFuelMoistureData initialFuelMoisture;
        
        public LCPData lcpData;
        public FuelModelSet fuelModelSet;
        public Surface surfaceFire;
        public Crown crownFire;

        private double timeSinceStart = 0.0;

        public WUInityFireMesh(string lcpFilename, WeatherInput weather, WindInput wind, InitialFuelMoistureData initialFuelMoisture, WUInityFireIgnition[] ignitionPoints)
        {
            lcpData = new LCPData(lcpFilename);
            cellSize = new Vector2D(lcpData.RasterCellResolutionX, lcpData.RasterCellResolutionY);
            cellCount = new Vector2Int(Mathf.CeilToInt((float)(WUInity.WUINITY_IN.size.x / cellSize.x)), Mathf.CeilToInt((float)(WUInity.WUINITY_IN.size.y / cellSize.x)));

            this.weather = weather;
            this.wind = wind;
            this.initialFuelMoisture = initialFuelMoisture;

            this.ignitionPoints = ignitionPoints;
        }

        public WUInityFireMesh(LCPData lcpData, WeatherInput weather, WindInput wind, InitialFuelMoistureData initialFuelMoisture, WUInityFireIgnition[] ignitionPoints)
        {
            this.lcpData = lcpData;
            cellCount = new Vector2Int(lcpData.Header.numeast, lcpData.Header.numnorth);
            cellSize = new Vector2D(lcpData.RasterCellResolutionX, lcpData.RasterCellResolutionY);

            this.weather = weather;
            this.wind = wind;
            this.initialFuelMoisture = initialFuelMoisture;

            this.ignitionPoints = ignitionPoints;
        }

        void InitializeMesh()
        {
            fuelModelSet = new FuelModelSet();
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
            neighborIndices = new Vector2Int[indexSize];
            neighborIndices[0] = Vector2Int.up;
            neighborIndices[1] = Vector2Int.right;
            neighborIndices[2] = Vector2Int.down;
            neighborIndices[3] = Vector2Int.left;
            if (indexSize >= 8)
            {
                neighborIndices[4] = Vector2Int.up + Vector2Int.right;
                neighborIndices[5] = Vector2Int.right + Vector2Int.down;
                neighborIndices[6] = Vector2Int.down + Vector2Int.left;
                neighborIndices[7] = Vector2Int.left + Vector2Int.up;
            }
            if (indexSize >= 16)
            {
                neighborIndices[8] = 2 * Vector2Int.up + Vector2Int.right;
                neighborIndices[9] = Vector2Int.up + 2 * Vector2Int.right;
                neighborIndices[10] = 2 * Vector2Int.right + Vector2Int.down;
                neighborIndices[11] = Vector2Int.right + 2 * Vector2Int.down;
                neighborIndices[12] = 2 * Vector2Int.down + Vector2Int.left;
                neighborIndices[13] = Vector2Int.down + 2 * Vector2Int.left;
                neighborIndices[14] = 2 * Vector2Int.left + Vector2Int.up;
                neighborIndices[15] = Vector2Int.left + 2 * Vector2Int.up;
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
            cellSizeDiagonal = cellSize.x * Math.Sqrt(2.0);
            cellSizeSquared = cellSize.x * cellSize.x;
            cellSizeDiagonalSquared = cellSizeDiagonal * cellSizeDiagonal;
            sixteenDist = Math.Sqrt(5.0) * cellSize.x;
            sixteenDistSquared = sixteenDist * sixteenDist;

            fireCells = new WUInityFireCell[cellCount.x * cellCount.y];
            for (int y = 0; y < cellCount.y; ++y)
            {
                for (int x = 0; x < cellCount.x; ++x)
                {                    
                    LandScapeStruct l = lcpData.GetCellData(x, y);
                    fireCells[GetCellIndex(x, y)] = new WUInityFireCell(this, x, y, l);
                }
            }
            //calc distances based on elevation etc
            for (int y = 0; y < cellCount.y; ++y)
            {
                for (int x = 0; x < cellCount.x; ++x)
                {
                    //send data to cell, fuel type etc
                    fireCells[GetCellIndex(x, y)].InitCell();
                }
            }
            activeCells = new HashSet<WUInityFireCell>();
            cellsToKill = new HashSet<WUInityFireCell>();
            cellsToIgnite = new HashSet<WUInityFireCell>();
            

            burnTexture = new Texture2D(cellCount.x, cellCount.y);
            for (int y = 0; y < cellCount.y; y++)
            {
                for (int x = 0; x < cellCount.x; x++)
                {
                    float tint = fireCells[GetCellIndex(x, y)].GetFuelModelNumber() / 13.0f;
                    Color c = Color.green * tint;
                    c.a = 0.4f;
                    burnTexture.SetPixel(x, y, c);

                    //tex.SetPixel(x, y, Color.white * (float)fireCells[GetCellIndex(x, y)].GetAspect() / 360.0f);
                    //tex.SetPixel(x, y, Color.white * (float)fireCells[GetCellIndex(x, y)].GetSlope() / 1000.0f);
                    //tex.SetPixel(x, y, Color.white * (float)fireCells[GetCellIndex(x, y)].GetElevation() / (256.0f * (float)cellSize));
                }
            }
            burnTexture.filterMode = FilterMode.Point;  
            
            if(WUInity.WUINITY != null)
            {
                WUInity.WUINITY.fireMaterial.mainTexture = burnTexture;
            }

            StartInitialIgnition();

            CreateTerrainPlane();
        }

        double GetCorrectedElevation(int x, int y)
        {            
            return fireCells[GetCellIndex(x, y)].GetElevation() - lcpData.Header.loelev;           
        }

        double GetRawElevation(int x, int y)
        {
            return fireCells[GetCellIndex(x, y)].GetElevation();
        }

        public void AddCellToIgnite(WUInityFireCell f)
        {
            if(!cellsToIgnite.Contains(f))
            {                
                cellsToIgnite.Add(f);
            }            
        }

        public void RemoveDeadCell(WUInityFireCell f)
        {
            cellsToKill.Add(f);            
        }

        public double GetAngleOffset(int i)
        {
            return angleOffsets[i];
        }

        bool initialized = false;
        public bool Simulate()
        {
            if(!initialized)
            {
                InitializeMesh();
                initialized = true;
            }

            if (activeCells.Count == 0)
            {
                return false;
            }            

            //collect max spread rate
            double maxSpreadRate = -1.0;
            double maxFireLineIntensity = -1.0;
            foreach (WUInityFireCell f in activeCells)
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
            dt = cellSize.x / maxSpreadRate;        

            //move fire fronts
            foreach (WUInityFireCell f in activeCells)
            {
                f.Burn();
            }

            //check if spread occurs, tags cells that gets ignited, "add" spill
            foreach (WUInityFireCell f in activeCells)
            {
                f.CheckFireSpread();
            }
                        
            //remove dead cells (that has nowhere to spread according to CheckFireSpread)
            foreach (WUInityFireCell f in cellsToKill)
            {
                activeCells.Remove(f);
                Color fireLineIntensityColor = GetFireLineIntensityColor(f, maxFireLineIntensity);
                burnTexture.SetPixel(f.cellIndex.x, f.cellIndex.y, fireLineIntensityColor);
            }
            cellsToKill.Clear();

            //add the ones that were ignited to the active list
            foreach (WUInityFireCell f in cellsToIgnite)
            {
                f.Ignite(timeSinceStart);
                activeCells.Add(f);
                burnTexture.SetPixel(f.cellIndex.x, f.cellIndex.y, Color.red);
            }
            cellsToIgnite.Clear();

            //second round of checking if dead since some neighbors might have changed status after kill & ignite loops
            foreach (WUInityFireCell f in activeCells)
            {
                f.CheckIfDead();
            }

            //remove dead cells (that has nowhere to spread)
            foreach (WUInityFireCell f in cellsToKill)
            {
                activeCells.Remove(f);
                Color fireLineIntensityColor = GetFireLineIntensityColor(f, maxFireLineIntensity);
                burnTexture.SetPixel(f.cellIndex.x, f.cellIndex.y, fireLineIntensityColor);
            }

            burnTexture.Apply();

            //update time and wind for next time step. TODO: spread out the update over several frames
            timeSinceStart += dt;
            currentWindData = wind.GetWindDataAtTime((float)timeSinceStart);
            //TODO: only update if any input has changed, re-calculate spread rates
            UpdateCellSpreadRates();

            return true;
        }     

        void UpdateCellSpreadRates()
        {
            foreach (WUInityFireCell f in activeCells)
            {
                f.UpdateSpreadRates();
            }
        }

        public void StartInitialIgnition()
        {
            for (int i = 0; i < ignitionPoints.Length; ++i)
            {
                ignitionPoints[i].CalculateMeshIndex(this);
                if (ignitionPoints[i].IsInsideFire(cellCount))
                {
                    int x = ignitionPoints[i].GetX();
                    int y = ignitionPoints[i].GetY();
                    WUInityFireCell f = fireCells[GetCellIndex(x, y)];
                    f.Ignite(0.0);
                    activeCells.Add(f);
                    burnTexture.SetPixel(x, y, Color.red);
                }
            }

            burnTexture.Apply();
        }

        int GetCellIndex(int x, int y)
        {
            //clamp
            x = Mathf.Clamp(x, 0, cellCount.x - 1);
            y = Mathf.Clamp(y, 0, cellCount.y - 1);
            
            return (x + y * cellCount.x);
        }

        public bool IsInsideMesh(int x, int y)
        {
            bool isInside = true;
            if (x < 0 || x > cellCount.x - 1 || y < 0 || y > cellCount.y - 1)
            {
                isInside = false;
            }
            return isInside;
        }

        public WUInityFireCell GetFireCell(int x, int y)
        {
            return fireCells[GetCellIndex(x, y)];
        }

        public FireCellState GetFireCellState(Vector2D latLong)
        {
            Mapbox.Utils.Vector2d pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(latLong.x, latLong.y, WUInity.WUINITY_MAP.CenterMercator, WUInity.WUINITY_MAP.WorldRelativeScale);

            int x = (int)(pos.x / cellSize.x);
            int y = (int)(pos.y / cellSize.x);

            if(!IsInsideMesh(x, y))
            {
                return FireCellState.Dead;
            }
            else
            {
                return fireCells[GetCellIndex(x, y)].cellState;
            }            
        }

        void CreateTerrainPlane()
        {
            terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            terrainMesh.Clear();
            CreatePlane(terrainMesh, cellCount.x, cellCount.y, (float)(cellCount.x * cellSize.x), (float)(cellCount.y * cellSize.y), 0.0f, Vector2.one, true);
        }

        private void CreatePlane(Mesh mesh, int cellsX, int cellsZ, float sizeX, float sizeZ, float yPos, Vector2 maxUV, bool getElevation)
        {
            //add one since we always need one extra, e.g..1 cell equals 4 vertices
            cellsX += 1;
            cellsZ += 1;

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
        }

        Color GetFireLineIntensityColor(WUInityFireCell cell, double maxIntensity)
        {
            float f = (float)cell.GetFireLineIntensity(true);
            f = f / 6000f;//(float)maxIntensity;
            Color color = Color.HSVToRGB(0.67f - 0.67f * f, 1.0f, 1.0f);
            color.a = 1f;
            return color;
        }

        public int GetCellSize()
        {
            return (int)cellSize.x;
        }

        public Vector2Int GetCellCount()
        {
            return cellCount;
        }

        public int[,] GetMaxROS()
        {
            int[,] ros = new int[cellCount.x * cellCount.y, 8];
            for (int i = 0; i < cellCount.x; i++)
            {
                for (int j = 0; j < cellCount.y; j++)
                {
                    WUInityFireCell f = fireCells[GetCellIndex(i, j)];
                    for (int k = 0; k < 8; k++)
                    {
                        ros[j + i * cellCount.y, k] = f.GetMaxSpreadrateInDirection(k);
                    }
                }
            }
            return ros;
        }
    }
}
