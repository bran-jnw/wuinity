using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Smoke
{
    /// <summary>
    /// Advects and diffuses (eddy diffusion) on a 2d plane with height set to assumed mixing height (which gives volume).
    /// Within each volume a uniform soot dsitribution is assumed (perfectly mixed).
    /// </summary>
    public class AdvectDiffuseModel
    {
        int cellCountX, cellCountY;
        float cellSizeX, cellSizeY;
        float cellArea, cellVolume, invertedCellVolume, cellHeight, invertedCellSizeX, invertedCellSizeY, invertedCellSizeXSq, invertedCellSizeYSq;
        Fire.FireMesh fireMesh;
        Fire.FireCell[] fireCellReferences;
        ComputeShader advectDiffuseCompute;
        ComputeBuffer[] sootConcentration;
        ComputeBuffer sootInjection;
        int solutionMode;

        const int READ = 0;
        const int WRITE = 1;
        const int NUM_THREADS_X = 8;
        const int NUM_THREADS_Y = 8;
        const int NUM_THREADS_Z = 1;

        public int GetCellsX()
        {
            return cellCountX;
        }

        public int GetCellsY()
        {
            return cellCountY;
        }

        //NOT USING ANY Vector2 SINCE THEY ARE SLOWER THAN NORMAL FLOATS (each .x or .y creates Vector2.get call)
        public AdvectDiffuseModel(Fire.FireMesh fireMesh, float mixingHeight, ComputeShader advectDiffuseCompute, Texture2D noiseTex, int solutionMode = 0)
        {
            this.fireMesh = fireMesh;
            //set all parameters
            cellCountX = fireMesh.cellCount.x;
            cellCountY = fireMesh.cellCount.y;
            cellSizeX = (float)fireMesh.cellSize.x;
            invertedCellSizeX = 1.0f / cellSizeX;
            invertedCellSizeXSq = invertedCellSizeX * invertedCellSizeX;
            cellSizeY = (float)fireMesh.cellSize.y;
            invertedCellSizeY = 1.0f / cellSizeY;
            invertedCellSizeYSq = invertedCellSizeY * invertedCellSizeY;
            cellHeight = mixingHeight;
            cellArea = cellSizeX * cellSizeY;
            cellVolume = cellSizeX * cellSizeY * cellHeight;
            invertedCellVolume = 1f / cellVolume;   

            //set compute shader and create buffers
            this.advectDiffuseCompute = advectDiffuseCompute;
            sootConcentration = new ComputeBuffer[2];
            sootConcentration[0] = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            sootConcentration[1] = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            sootInjection = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));

            this.solutionMode = 1;//solutionMode;
            //set constants in compute buffer
            advectDiffuseCompute.SetInt("_CellsX", cellCountX);
            advectDiffuseCompute.SetInt("_CellsY", cellCountY);
            advectDiffuseCompute.SetFloat("_InvertedCellVolume", invertedCellVolume);
            advectDiffuseCompute.SetFloat("_CellsPerMeterX", invertedCellSizeX);
            advectDiffuseCompute.SetFloat("_CellsPerMeterY", invertedCellSizeY);
            advectDiffuseCompute.SetFloat("_CellsPerMeterXSq", invertedCellSizeXSq);
            advectDiffuseCompute.SetFloat("_CellsPerMeterYSq", invertedCellSizeYSq);
            advectDiffuseCompute.SetTexture(solutionMode, "_NoiseTex", noiseTex, 0);
        }

        public void Update(float deltaTime, float windDirection, float windSpeed, bool fireHasUpdated)
        {
            //due to definition of wind direction we rotate 180 degrees to get magnitudes correct
            windDirection -= 180f;
            //swap cos and sin since 0 degree wind is from north while unit circle has 0 at east
            float windX = Mathf.Sin(windDirection * Mathf.Deg2Rad) * windSpeed;
            float windY = Mathf.Cos(windDirection * Mathf.Deg2Rad) * windSpeed;
            float windXFraction = windX / windSpeed;
            float windYFraction = windY / windSpeed;
            float absoluteWindX = Mathf.Abs(windX);
            float absoluteWindY = Mathf.Abs(windY);

            //to keep it stable we use a CFL number < 1 and use internal partial time steps
            float maxCFL = 0.7f;
            float courantAdvection = deltaTime * (absoluteWindX / cellSizeX + absoluteWindY / cellSizeY);
            float maxDiffusion = 22f;
            //TODO: this is not correct, blows up with too high diffusion terms
            float courantDiffusion = 2 * maxDiffusion * deltaTime * (invertedCellSizeXSq + invertedCellSizeYSq);
            float internalTimeSteps = Mathf.CeilToInt(Mathf.Max(courantAdvection, courantDiffusion) / maxCFL);
            float internalDeltaTime = deltaTime / internalTimeSteps;
            //MonoBehaviour.print("Courant Adv.: " + courantAdvection + ", Courant Diff.: " + courantDiffusion + ", intenal timesteps; " + internalTimeSteps + ", internal delta time: " + internalDeltaTime);

            //Set all of the data in compute shader
            advectDiffuseCompute.SetFloat("_DeltaTime", internalDeltaTime);
            advectDiffuseCompute.SetFloat("_WindX", windX);
            advectDiffuseCompute.SetFloat("_WindY", windY);
            advectDiffuseCompute.SetFloat("_WindXFraction", windXFraction);
            advectDiffuseCompute.SetFloat("_WindYFraction", windYFraction);

            //if fire has been updated we need to update the injection buffer
            if (fireHasUpdated)
            {
                float[] sootInjectionCPU = fireMesh.GetSootProduction();
                sootInjection.SetData(sootInjectionCPU);
                advectDiffuseCompute.SetBuffer(solutionMode, "_SootInjection", sootInjection);
            }
            
            //go through time steps and update soot concetration in compute shader
            for (int i = 0; i < internalTimeSteps; i++)
            {
                //as we swap these they have to be set every time
                advectDiffuseCompute.SetBuffer(solutionMode, "_Read", sootConcentration[READ]);
                advectDiffuseCompute.SetBuffer(solutionMode, "_Write", sootConcentration[WRITE]);
                advectDiffuseCompute.Dispatch(solutionMode, Mathf.CeilToInt(cellCountX / (float)NUM_THREADS_X), Mathf.CeilToInt(cellCountY / (float)NUM_THREADS_Y), NUM_THREADS_Z);
                //after calculation we need to swap read/write to save new values
                Swap(sootConcentration);
            }
        }

        void Swap(ComputeBuffer[] buffer)
        {
            ComputeBuffer tmp = buffer[READ];
            buffer[READ] = buffer[WRITE];
            buffer[WRITE] = tmp;
        }

        public ComputeBuffer GetComputeBuffer()
        {
            //return the read buffer as that contains the updated information after the swap
            return sootConcentration[0];
        }

        public void Release()
        {
            for (int i = 0; i < sootConcentration.Length; i++)
            {   
                if(sootConcentration[i] != null)
                {
                    sootConcentration[i].Release();
                    sootConcentration[i] = null;
                }                
            }

            if (sootInjection != null)
            {
                sootInjection.Release();
                sootInjection = null;
            }
        }
    }
}


