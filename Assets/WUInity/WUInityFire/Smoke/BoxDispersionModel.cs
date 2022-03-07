using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace WUInity.Smoke
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
        Texture2D concentrationTexture;
        Fire.FireCell[] fireCellReferences;
        float xFaceArea, yFaceArea;
        int paddedCellCountX, paddedCellCountY, cellCount;


        //NOT USING ANY Vector2 SINCE THEY ARE SLOWER THAN NORMAL FLOATS (each .x or .y creates Vector2.get call)
        public BoxDispersionModel(Fire.FireMesh fireMesh, float height = 100.0f)
        {
            this.fireMesh = fireMesh;
            cellCountX = fireMesh.cellCount.x;
            cellCountY = fireMesh.cellCount.y;
            cellSizeX = (float)fireMesh.cellSize.x;
            cellSizeY = (float)fireMesh.cellSize.y;
            cellHeight = height;
            cellArea = cellSizeX * cellSizeY;
            cellVolume = cellSizeX * cellSizeY * height;
            invertedCellVolume = 1f / cellVolume;
            cellCount = cellCountX * cellCountY;
            concentration = new float[(cellCountX + 2) * (cellCountY + 2)]; //added padding around to get outside range faster
            concentrationBuffer = new float[cellCount];
            concentrationTexture = new Texture2D(cellCountX, cellCountY);
            xFaceArea = cellSizeY * cellHeight;
            yFaceArea = cellSizeX * cellHeight;

            paddedCellCountX = cellCountX + 2;
            paddedCellCountY = cellCountY + 2;
            computeBuffer = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
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

        public void Update(float deltaTime, float windDirection, float windSpeed)
        {
            if(fireCellReferences == null)
            {
                CacheFireCells();
            }

            windDirection -= 180f;
            float windX = Mathf.Sin(windDirection * Mathf.Deg2Rad) * windSpeed;
            float windY = Mathf.Cos(windDirection * Mathf.Deg2Rad) * windSpeed;
            float absoluteWindX = Mathf.Abs(windX);
            float absoluteWindY = Mathf.Abs(windY); 
            float maxConcentration = float.MinValue;

            //to keep it stable we use a CFL number of 0.5 and use internal partial time steps
            float CFL = 0.5f;
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
                        if (fireCell.cellState == Fire.FireCellState.Burning) // || fireCell.cellState == Fire.FireCellState.Dead)
                        {
                            //when done testing, move this calc to the fire cell itself, more effective sine less frequent updates
                            QA = 0.1f * cellArea * (float)fireCell.GetReactionIntensity() / 21500.0f; //intensity is kW/m2, assume 21 500 kJ/kg HOC, soot yield 0.1
                        }

                        //padded around with zeroes so we need to fix index
                        int paddedX = x + 1;
                        int yInside = y + 1;
                        float center = concentration[paddedX + yInside * paddedCellCountX];
                        float left = concentration[paddedX - 1 + yInside * paddedCellCountX];
                        float right = concentration[paddedX + 1 + yInside * paddedCellCountX];
                        float down = concentration[paddedX + (yInside - 1) * paddedCellCountX];
                        float up = concentration[paddedX + (yInside + 1) * paddedCellCountX];

                        float incomingX = windX >= 0f ? left : right;
                        float incomingY = windY >= 0f ? down : up;
                        //float outgoingX = wind.x < 0f ? left : right;
                        //float outgoingY = wind.y < 0f ? down : up;                        
                        float xDelta = absoluteWindX * yFaceArea * (incomingX - center);
                        float yDelta = absoluteWindY * xFaceArea * (incomingY - center);

                        /*//advection
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
                        float advection = advectionX + advectionY;

                        //diffusion
                        float eddyDiffusionCoefficient = 10f;
                        float diffusionX = (right - 2 * center + left) / (cellSizeX * cellSizeX);
                        float diffusionY = (up - 2 * center + down) / (cellSizeY * cellSizeY);
                        float diffusion = eddyDiffusionCoefficient * (diffusionX + diffusionY);*/

                        float C_delta = QA + xDelta + yDelta;
                        C_delta *= internalDeltaTime * invertedCellVolume;
                        //C_delta = internalDeltaTime * (-advection + diffusion + QA);
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

                        //we use this for tmeporary storage when giving to GPU
                        concentrationBuffer[smallIndex] = concentration[bigIndex];

                        if (concentration[bigIndex] > maxConcentration)
                        {
                            maxConcentration = concentration[bigIndex];
                        }
                    }
                }
            }

            //UpdateTexture(maxConcentration);
        }

        private void UpdateTexture(float maxConcentration)
        {
            for (int y = 0; y < cellCountY; y++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    int index = (x + 1) + (y + 1) * paddedCellCountX;
                    float lightExtinction = 1.2f * 8700f * concentration[index];
                    Color c = new Color(lightExtinction, 0f, 0f, 0.5f);
                    concentrationTexture.SetPixel(x, y, c);
                }
            }
            concentrationTexture.Apply();
        }

        public Texture2D GetConcentrationTexture()
        {
            return concentrationTexture;
        }

        ComputeBuffer computeBuffer;
        public void UpdateVisualization(Material boxDispersionMaterial)
        {            
            computeBuffer.SetData(concentrationBuffer);
            boxDispersionMaterial.SetBuffer("_Data", computeBuffer);
            boxDispersionMaterial.SetInteger("_CellsX", cellCountX);
            boxDispersionMaterial.SetInteger("_CellsY", cellCountY);
        }

        ~BoxDispersionModel()  // finalizer
        {
            computeBuffer.Release();
            computeBuffer = null;
        }
    }
}

