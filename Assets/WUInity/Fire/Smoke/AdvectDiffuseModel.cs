using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Smoke
{
    /// <summary>
    /// Advects and diffuses (eddy diffusion, turbulent viscosity) on a 2d plane with height set to assumed mixing height (which gives volume).
    /// Within each volume a uniform soot distribution is assumed (perfectly mixed).
    /// </summary>
    public class AdvectDiffuseModel
    {
        int cellCountX, cellCountY;
        float cellSizeX, cellSizeY;
        float cellArea, cellVolume, invertedCellVolume, cellHeight, invertedCellSizeX, invertedCellSizeY, invertedCellSizeXSq, invertedCellSizeYSq;
        Fire.FireMesh fireMesh;
        ComputeShader advectDiffuseCompute;
        ComputeBuffer[] sootConcentration;
        ComputeBuffer sootInjection;
        ComputeBuffer wind;
        int solutionMode;

        const int READ = 0;
        const int WRITE = 1;
        //if these are ever changed they also have to be changed in compute shaders
        const int NUM_THREADS_X = 8;
        const int NUM_THREADS_Y = 8;
        const int NUM_THREADS_Z = 1;

        ~AdvectDiffuseModel()
        {
            Release();
        }

        public int GetCellsX()
        {
            return cellCountX;
        }

        public int GetCellsY()
        {
            return cellCountY;
        }

        //NOT USING ANY Vector2 SINCE THEY ARE SLOWER THAN NORMAL FLOATS (each .x or .y creates Vector2.get call)
        public AdvectDiffuseModel(Fire.FireMesh fireMesh, float mixingHeight, ComputeShader advectDiffuseCompute, Texture2D noiseTex, Texture2D windTex, int solutionMode = 0)
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
            wind = new ComputeBuffer(cellCountX * cellCountY, sizeof(float) * 2);

            this.solutionMode = 1;//solutionMode, 0 = explicit, 1 = semi-lagrangian
            //set constants in compute buffer
            advectDiffuseCompute.SetInt("_CellsX", cellCountX);
            advectDiffuseCompute.SetInt("_CellsY", cellCountY);
            advectDiffuseCompute.SetFloat("_InvertedCellVolume", invertedCellVolume);
            advectDiffuseCompute.SetFloat("_CellSizeX", cellSizeX);
            advectDiffuseCompute.SetFloat("_CellSizeY", cellSizeY);
            advectDiffuseCompute.SetFloat("_CellSizeXSq", cellSizeX * cellSizeX);
            advectDiffuseCompute.SetFloat("_CellSizeYSq", cellSizeY * cellSizeY);
            advectDiffuseCompute.SetFloat("_CellsPerMeterX", invertedCellSizeX);
            advectDiffuseCompute.SetFloat("_CellsPerMeterY", invertedCellSizeY);
            advectDiffuseCompute.SetFloat("_CellsPerMeterXSq", invertedCellSizeXSq);
            advectDiffuseCompute.SetFloat("_CellsPerMeterYSq", invertedCellSizeYSq);
            advectDiffuseCompute.SetTexture(solutionMode, "_NoiseTex", noiseTex, 0);

            CreateWind(windTex);            
        }

        void CreateWind(Texture2D windTex)
        {
            advectDiffuseCompute.SetTexture(solutionMode, "_Wind", windTex);
            //we also need it in diffusion kernel if running semi-lagrangian mode
            if(solutionMode == 1)
            {
                advectDiffuseCompute.SetTexture(2, "_Wind", windTex);
                advectDiffuseCompute.SetTexture(3, "_Wind", windTex);
            }

            ComputeShader cs;
            string shaderName = "name_of_shader";
            ComputeShader[] compShaders = (ComputeShader[])Resources.FindObjectsOfTypeAll(typeof(ComputeShader));
            for (int i = 0; i < compShaders.Length; i++)
            {
                if (compShaders[i].name == shaderName)
                {
                    cs = compShaders[i];
                    break;
                }
            }

            //advectDiffuseCompute.SetBuffer(, "_Wind", wind);
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

            float internalTimeSteps = 1;
            float internalDeltaTime = deltaTime;
            if (solutionMode == 0)
            {
                //to keep it stable we use a CFL number < 1 and use internal partial time steps
                float maxCFL = 0.7f;
                float courantAdvection = deltaTime * (absoluteWindX / cellSizeX + absoluteWindY / cellSizeY);
                float maxDiffusion = 22f;
                //TODO: this is not correct, blows up with too high diffusion terms
                float courantDiffusion = 2 * maxDiffusion * deltaTime * (invertedCellSizeXSq + invertedCellSizeYSq);
                internalTimeSteps = Mathf.CeilToInt(Mathf.Max(courantAdvection, courantDiffusion) / maxCFL);
                internalDeltaTime = deltaTime / internalTimeSteps;
                //MonoBehaviour.print("Courant Adv.: " + courantAdvection + ", Courant Diff.: " + courantDiffusion + ", intenal timesteps; " + internalTimeSteps + ", internal delta time: " + internalDeltaTime);
            }

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

            int diffuseKernel = 3; // 2 IS ISOTROPIC, 3 IS ANISOTROPIC
            //go through time steps and update soot concetration in compute shader
            for (int i = 0; i < internalTimeSteps; i++)
            {
                //as we swap these they have to be set every time
                advectDiffuseCompute.SetBuffer(solutionMode, "_Read", sootConcentration[READ]);
                advectDiffuseCompute.SetBuffer(solutionMode, "_Write", sootConcentration[WRITE]);
                advectDiffuseCompute.Dispatch(solutionMode, Mathf.CeilToInt(cellCountX / (float)NUM_THREADS_X), Mathf.CeilToInt(cellCountY / (float)NUM_THREADS_Y), NUM_THREADS_Z);
                //after calculation we need to swap read/write to save new values
                Swap(sootConcentration);

                //if we are using the semi-lagrangian solver we then also call the diffusion kernel
                if (solutionMode == 1)
                {
                    //implicit jacobian iterations
                    for (int j = 0; j < 20; j++)
                    {
                        advectDiffuseCompute.SetBuffer(diffuseKernel, "_Read", sootConcentration[READ]);
                        advectDiffuseCompute.SetBuffer(diffuseKernel, "_Write", sootConcentration[WRITE]);
                        advectDiffuseCompute.Dispatch(diffuseKernel, Mathf.CeilToInt(cellCountX / (float)NUM_THREADS_X), Mathf.CeilToInt(cellCountY / (float)NUM_THREADS_Y), NUM_THREADS_Z);
                        Swap(sootConcentration);
                    }
                }  
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


