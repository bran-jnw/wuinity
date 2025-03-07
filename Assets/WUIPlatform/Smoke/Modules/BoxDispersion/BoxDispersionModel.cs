//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform.Smoke
{
    /// <summary>
    /// Based on https://www.sciencedirect.com/topics/engineering/box-model
    /// </summary>
    public class BoxDispersionModel
    {
        int cellCountX, cellCountY;
        float cellSizeX, cellSizeY;
        float cellArea, cellVolume, invertedCellVolume, cellHeight;
        Fire.FireMesh fireMesh;
        float[] concentration;
        float[] concentrationBuffer;
        Fire.FireCell[] fireCellReferences;
        int paddedCellCountX, cellCount;


        //NOT USING ANY Vector2 SINCE THEY ARE SLOWER THAN NORMAL FLOATS (each .x or .y creates Vector2.get call)
        public BoxDispersionModel(Fire.FireMesh fireMesh, float height = 250.0f)
        {
            this.fireMesh = fireMesh;
            cellCountX = fireMesh.GetCellCountX();
            cellCountY = fireMesh.GetCellCountY();
            cellSizeX = (float)fireMesh._cellSize.x;
            cellSizeY = (float)fireMesh._cellSize.y;
            cellHeight = height;
            cellArea = cellSizeX * cellSizeY;
            cellVolume = cellSizeX * cellSizeY * height;
            invertedCellVolume = 1f / cellVolume;
            cellCount = cellCountX * cellCountY;
            paddedCellCountX = cellCountX + 2;
            concentration = new float[paddedCellCountX * (cellCountY + 2)]; //added padding around to get outside range faster
            concentrationBuffer = new float[cellCount];                     
        }       
        
        void CacheFireCells()
        {
            //need to cache the postions as it is really slow to get them...
            fireCellReferences = new Fire.FireCell[cellCount];
            for (int i = 0; i < cellCount; i++)
            {
                fireCellReferences[i] = fireMesh.GetFireCell(i);
            }
        }

        public int GetCellsX()
        {
            return cellCountX;
        }

        public int GetCellsY()
        {
            return cellCountY;
        }

        public float[] GetData()
        {
            return concentrationBuffer;
        }

        public void Update(float deltaTime, float windDirection, float windSpeed)
        {
            if(fireCellReferences == null)
            {
                CacheFireCells();
            }

            windDirection -= 180f;
            float windX = Mathf.Sin(windDirection * Mathf.Deg2Rad) * windSpeed;
            float windY = Mathf.Cos(windDirection * Mathf.Deg2Rad) * windSpeed;
            float windXFraction = windX / windSpeed;
            float windYFraction = windY / windSpeed;
            float absoluteWindX = Mathf.Abs(windX);
            float absoluteWindY = Mathf.Abs(windY); 
            float maxConcentration = float.MinValue;

            //to keep it stable we use a CFL number of 0.5 and use internal partial time steps
            float CFL = 0.4f;
            int internalTimeSteps = Mathf.CeilToInt(deltaTime / Mathf.Min(CFL * cellSizeX / absoluteWindX, CFL * cellSizeY / absoluteWindY));
            float internalDeltaTime = deltaTime / internalTimeSteps;

            for (int i = 0; i < internalTimeSteps; i++)
            {
                //calculate the change in each cell
                for (int y = 0; y < cellCountY; y++)
                {
                    for (int x = 0; x < cellCountX; x++)
                    {
                        Fire.FireCell fireCell = fireCellReferences[x + y * cellCountX];
                        float QA = 0.0f;
                        if (fireCell.cellState == Fire.FireCellState.Burning) //|| fireCell.cellState == Fire.FireCellState.Dead)
                        {
                            //when done testing, move this calc to the fire cell itself, more effective sine less frequent updates
                            QA = 0.015f * cellArea * (float)fireCell.GetReactionIntensity() / 21500.0f; //intensity is kW/m2, assume 21 500 kJ/kg HOC, soot yield 0.015 for wood founf for FDS
                        }

                        //padded around with zeroes so we need to fix index
                        int paddedX = x + 1;
                        int paddedY = y + 1;
                        float center = concentration[paddedX + paddedY * paddedCellCountX];
                        float left = concentration[paddedX - 1 + paddedY * paddedCellCountX];
                        float right = concentration[paddedX + 1 + paddedY * paddedCellCountX];
                        float down = concentration[paddedX + (paddedY - 1) * paddedCellCountX];
                        float up = concentration[paddedX + (paddedY + 1) * paddedCellCountX];

                        //advection up-wind scheme
                        float advectionX = windX * (center - left) / (cellSizeX);
                        if(windX < 0)
                        {
                            advectionX = windX * (right - center) / (cellSizeX);
                        }
                        float advectionY = windY * (center - down) / (cellSizeY);
                        if(windY < 0)
                        {
                            advectionY = windY * (up - center) / (cellSizeY);
                        }
                        //TODO: central difference blows up
                        //advectionX = windX * (right - left) / (2 * cellSizeX);
                        //advectionY = windY * (up - down) / (2 * cellSizeY);
                        float advection = advectionX + advectionY;

                        //diffusion
                        float eddyDiffusionCoefficientAlong = 2f;
                        float eddyDiffusionCoefficientAcross = 10f;
                        float diffusionX = (right - 2 * center + left) / (cellSizeX * cellSizeX);
                        float diffusionY = (up - 2 * center + down) / (cellSizeY * cellSizeY);
                        diffusionX *= (eddyDiffusionCoefficientAlong * windXFraction + eddyDiffusionCoefficientAcross * (1.0f - windXFraction));
                        diffusionY *= (eddyDiffusionCoefficientAlong * windYFraction + eddyDiffusionCoefficientAcross * (1.0f - windYFraction));
                        float diffusion = diffusionX + diffusionY;

                        float C_delta = internalDeltaTime * (-advection + diffusion + QA * invertedCellVolume);
                        concentrationBuffer[x + y * cellCountX] = center + C_delta;
                    }
                }
                
                //then apply the change               
                for (int y = 0; y < cellCountY; y++)
                {
                    for (int x = 0; x < cellCountX; x++)
                    {
                        int bigIndex = (x + 1) + (y + 1) * paddedCellCountX;
                        int smallIndex = x + y * cellCountX;
                        concentration[bigIndex] = concentrationBuffer[smallIndex];

                        if (concentration[bigIndex] > maxConcentration)
                        {
                            maxConcentration = concentration[bigIndex];
                        }
                    }
                }
            }
        }
    }
}

