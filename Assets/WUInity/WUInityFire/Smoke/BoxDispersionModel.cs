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
        float[,] concentration;
        float[,] concentrationChange;
        float[] linearArray; //for use in shaders
        Texture2D concentrationTexture;
        Fire.FireCell[,] fireCellReferences;
        float xFaceArea, yFaceArea;


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
            concentration = new float[cellCountX + 2 , cellCountY + 2]; //added padding around to get outside range faster
            concentrationChange = new float[cellCountX, cellCountY];
            linearArray = new float[cellCountX * cellCountY];
            concentrationTexture = new Texture2D(cellCountX, cellCountY);
            xFaceArea = cellSizeY * cellHeight;
            yFaceArea = cellSizeX * cellHeight;
        }       
        
        void CacheFireCells()
        {
            //need to cache the postions as it is really slow to get them...
            fireCellReferences = new Fire.FireCell[cellCountX, cellCountY];
            for (int y = 0; y < cellCountY; y++)
            {
                for (int x = 0; x < cellCountX; x++)
                {
                    fireCellReferences[x, y] = fireMesh.GetFireCell(x, y);
                }
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
                        Fire.FireCell fireCell = fireCellReferences[x, y];
                        float QA = 0.0f;

                        if (fireCell.cellState == Fire.FireCellState.Burning || fireCell.cellState == Fire.FireCellState.Dead)
                        {
                            //when done testing, move this calc to the fire cell itself, more effective sine less frequent updates
                            QA = 0.05f * cellArea * (float)fireCell.GetReactionIntensity() / 21500.0f; //intensity is kW/m2, assume 21 500 kJ/kg HOC, soot yield 0.05
                        }

                        //padded around with zeroes
                        float center = concentration[x + 1, y + 1];
                        //this is safe since padding around the array is added (containg zeroes)
                        float left = concentration[x, y + 1];
                        float right = concentration[x + 2, y + 1];
                        float down = concentration[x + 1, y];
                        float up = concentration[x + 1, y + 2];

                        float incomingX = windX >= 0f ? left : right;
                        float incomingY = windY >= 0f ? down : up;
                        //float outgoingX = wind.x < 0f ? left : right;
                        //float outgoingY = wind.y < 0f ? down : up;                        
                        float xDelta = absoluteWindX * yFaceArea * (incomingX - center);
                        float yDelta = absoluteWindY * xFaceArea * (incomingY - center);
                        float C_delta = QA + xDelta + yDelta;
                        C_delta *= internalDeltaTime * invertedCellVolume;
                        concentrationChange[x, y] = C_delta;
                    }
                }
                
                //then apply the change               
                for (int y = 0; y < cellCountY; y++)
                {
                    for (int x = 0; x < cellCountX; x++)
                    {
                        concentration[x + 1, y + 1] = concentration[x + 1, y + 1] + concentrationChange[x, y]; //Mathf.Max(0, 
                        int index = x + y * cellCountX;
                        //used in shaders
                        linearArray[index] = 1.2f * 8700f * concentration[x + 1, y + 1];

                        if (concentration[x + 1, y + 1] > maxConcentration)
                        {
                            maxConcentration = concentration[x, y];
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
                    float lightExtinction = 1.2f * 8700f * concentration[x + 1, y + 1];
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

        public float[] GetConcentrationData()
        {
            return linearArray;
        }

        ComputeBuffer buffer;
        public void UpdateVisualization(Material boxDispersionMaterial)
        {
            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
            buffer = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            buffer.SetData(linearArray);
            boxDispersionMaterial.SetBuffer("_Data", buffer);
            boxDispersionMaterial.SetInteger("_CellsX", cellCountX);
            boxDispersionMaterial.SetInteger("_CellsY", cellCountY);
        }

        ~BoxDispersionModel()  // finalizer
        {
            buffer.Release();
            buffer = null;
        }
    }
}

