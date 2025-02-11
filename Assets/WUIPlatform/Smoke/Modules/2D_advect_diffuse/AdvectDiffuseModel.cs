//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using UnityEngine;

namespace WUIPlatform.Smoke
{
    /// <summary>
    /// Advects and diffuses (eddy diffusion, turbulent viscosity) on a 2d plane with height set to assumed mixing height (which gives volume).
    /// Within each volume a uniform soot distribution is assumed (perfectly mixed).
    /// </summary>
    public class AdvectDiffuseModel : SmokeModule
    {
        int cellCountX, cellCountY, cellCountZ;
        float cellSizeX, cellSizeY, cellSizeZ;
        float cellArea, cellVolume, invertedCellVolume, cellHeight, invertedCellSizeX, invertedCellSizeY, invertedCellSizeXSq, invertedCellSizeYSq;
        Fire.FireModule fireModule;
        ComputeShader _advectDiffuseCompute;
        ComputeBuffer[] _sootConcentration;
        ComputeBuffer _sootInjection;
        ComputeBuffer _wind;
        ComputeBuffer _sigma;
        ComputeBuffer _random;
        int solutionMode;
        List<ComputeBuffer> _allBuffers;

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

        public override int GetCellsX()
        {
            return cellCountX;
        }

        public override int GetCellsY()
        {
            return cellCountY;
        }

        //NOT USING ANY Vector2 SINCE THEY ARE SLOWER THAN NORMAL FLOATS (each .x or .y creates Vector2.get call)
        public AdvectDiffuseModel(Fire.FireModule fireModule, float mixingHeight, ComputeShader advectDiffuseCompute, Texture2D noiseTex, Texture2D windTex, int solutionMode = 0)
        {
            _originOffset = fireModule.GetOriginOffset();

            this.fireModule = fireModule;
            //set all parameters
            cellCountX = fireModule.GetCellCountX();
            cellCountY = fireModule.GetCellCountY();
            cellSizeX = fireModule.GetCellSizeX();
            invertedCellSizeX = 1.0f / cellSizeX;
            invertedCellSizeXSq = invertedCellSizeX * invertedCellSizeX;
            cellSizeY = fireModule.GetCellSizeY();
            invertedCellSizeY = 1.0f / cellSizeY;
            invertedCellSizeYSq = invertedCellSizeY * invertedCellSizeY;
            cellHeight = mixingHeight;
            cellArea = cellSizeX * cellSizeY;
            cellVolume = cellSizeX * cellSizeY * cellHeight;
            invertedCellVolume = 1f / cellVolume;   

            //set compute shader and create buffers
            this._advectDiffuseCompute = advectDiffuseCompute;
            _sootConcentration = new ComputeBuffer[2];
            _sootConcentration[0] = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            _sootConcentration[1] = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            _sootInjection = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            _wind = new ComputeBuffer(cellCountX * cellCountY, sizeof(float) * 2);

            //save all buffers for easy disposal
            _allBuffers = new List<ComputeBuffer>();
            _allBuffers.Add(_sootConcentration[0]);
            _allBuffers.Add(_sootConcentration[1]);
            _allBuffers.Add(_sootInjection);
            _allBuffers.Add(_wind);


            this.solutionMode = 1;//solutionMode, 0 = advect and diffuse explicit, 1 = semi-lagrangian + implicit diffuse, 4 = new explicit, 5 = new explicit with flux limiter, 7 = semi-lagrangian + explicit diffuse
            if (this.solutionMode == 5)
            {
                _sigma = new ComputeBuffer(cellCountX * cellCountY, 3 * sizeof(float));
            }
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
            CreateRandom();
        }

        void CreateWind(Texture2D windTex)
        {
            _advectDiffuseCompute.SetTexture(solutionMode, "_Wind", windTex);
            //we also need it in diffusion kernel if running semi-lagrangian mode
            if(solutionMode == 1)
            {
                _advectDiffuseCompute.SetTexture(2, "_Wind", windTex);
                _advectDiffuseCompute.SetTexture(3, "_Wind", windTex);
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

        /// <summary>
        /// Generates random between -1 and 1 which can be used in combination with SigmaTheta 
        /// from https://www.ready.noaa.gov/READYpgclass.php to vary wind direction
        /// </summary>
        void CreateRandom()
        {
            _random = new ComputeBuffer(cellCountX * cellCountY, sizeof(float));
            _allBuffers.Add(_random);

            float[] randomData = new float[cellCountX * cellCountY];
            for (int i = 0; i < cellCountX * cellCountY; i++)
            {
                randomData[i] = Random.Range(-1.0f, 1.0f);
            }
            _random.SetData(randomData);
        }

        void UpdateExplicitTimeSteps2D(float windX, float windY, float maxK_H, float deltaTime, ref int internalTimeSteps, ref float internalDeltaTime)
        {
            float minDeltaTime = 1.0f / (2.0f * (maxK_H / (cellSizeX * cellSizeX) + maxK_H / (cellSizeY * cellSizeY)) + (windX / cellSizeX) + (windY / cellSizeY));
            //float minDeltaTime = 1.0f / (2.0f * (maxK_H / (cellSizeX * cellSizeX) + maxK_H / (cellSizeY * cellSizeY)) + (0.5f * windX * windX / cellSizeX) + (0.5f * windY * windY / cellSizeY));
            internalTimeSteps = Mathf.Max(1, Mathf.CeilToInt(deltaTime / (minDeltaTime * 0.9f)));
            internalDeltaTime = deltaTime / internalTimeSteps;
        }

        void UpdateExplicitTimeSteps3D(float windX, float windY, float windZ, float maxK_H, float maxK_V, float deltaTime, ref int internalTimeSteps, ref float internalDeltaTime)
        {
            float minDeltaTime = 1.0f / (2.0f * (maxK_H / (cellSizeX * cellSizeX) + maxK_H / (cellSizeY * cellSizeY) + maxK_V / (cellSizeZ * cellSizeZ)) + (windX / cellSizeX) + (windY / cellSizeY) + (windZ / cellSizeZ));
            internalTimeSteps = Mathf.Max(1, Mathf.CeilToInt(deltaTime / minDeltaTime));
            internalDeltaTime = deltaTime / internalTimeSteps;
        }

        void UpdateExplicitTimeSteps2DDiffusion(float maxK_H, float deltaTime, ref int internalTimeSteps, ref float internalDeltaTime)
        {
            float minDeltaTime = 0.25f * cellSizeX * cellSizeX / (4 * maxK_H);
            internalTimeSteps = Mathf.Max(1, Mathf.CeilToInt(deltaTime / (minDeltaTime * 0.9f)));
            internalDeltaTime = deltaTime / internalTimeSteps;
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

            int internalTimeSteps = 1;
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
            else if(solutionMode == 4 || solutionMode == 5)
            {
                UpdateExplicitTimeSteps2D(windX, windY, 50.0f, deltaTime, ref internalTimeSteps, ref internalDeltaTime);
            }
            else if(solutionMode == 7)
            {
                UpdateExplicitTimeSteps2DDiffusion(100.0f, deltaTime, ref internalTimeSteps, ref internalDeltaTime);
            }
                
            //Set all of the global data in compute shader
            _advectDiffuseCompute.SetFloat("_DeltaTime", internalDeltaTime);
            _advectDiffuseCompute.SetFloat("_WindX", windX);
            _advectDiffuseCompute.SetFloat("_WindY", windY);
            _advectDiffuseCompute.SetFloat("_WindXFraction", windXFraction);
            _advectDiffuseCompute.SetFloat("_WindYFraction", windYFraction);

            //if fire has been updated we need to update the injection buffer
            if (fireHasUpdated)
            {
                float[] sootInjectionCPU = fireModule.GetSootProduction();
                _sootInjection.SetData(sootInjectionCPU);
                _advectDiffuseCompute.SetBuffer(solutionMode, "_SootInjection", _sootInjection);
            }

            int diffuseKernel = 2; // 2 IS ISOTROPIC, 3 IS ANISOTROPIC
            //go through time steps and update soot concetration in compute shader
            for (int i = 0; i < internalTimeSteps; i++)
            {
                //calc sigma when using flux limiter
                if(solutionMode == 5)
                {
                    //sigma kernel is 6
                    _advectDiffuseCompute.SetBuffer(6, "_Read", _sootConcentration[READ]);
                    _advectDiffuseCompute.SetBuffer(6, "_Sigma3D", _sigma);
                    _advectDiffuseCompute.Dispatch(6, Mathf.CeilToInt(cellCountX / (float)NUM_THREADS_X), Mathf.CeilToInt(cellCountY / (float)NUM_THREADS_Y), NUM_THREADS_Z);
                    //set buffer in actual kernel
                    _advectDiffuseCompute.SetBuffer(solutionMode, "_Sigma3D", _sigma);
                }

                //as we swap these they have to be set every time
                _advectDiffuseCompute.SetBuffer(solutionMode, "_Read", _sootConcentration[READ]);
                _advectDiffuseCompute.SetBuffer(solutionMode, "_Write", _sootConcentration[WRITE]);
                _advectDiffuseCompute.Dispatch(solutionMode, Mathf.CeilToInt(cellCountX / (float)NUM_THREADS_X), Mathf.CeilToInt(cellCountY / (float)NUM_THREADS_Y), NUM_THREADS_Z);
                //after calculation we need to swap read/write to save new values
                Swap(_sootConcentration);

                //if we are using the semi-lagrangian solver with implicit diffusion we then also call the diffusion kernel
                if (solutionMode == 1)
                {
                    //implicit jacobian iterations
                    for (int j = 0; j < 20; j++)
                    {
                        _advectDiffuseCompute.SetBuffer(diffuseKernel, "_Read", _sootConcentration[READ]);
                        _advectDiffuseCompute.SetBuffer(diffuseKernel, "_Write", _sootConcentration[WRITE]);
                        _advectDiffuseCompute.Dispatch(diffuseKernel, Mathf.CeilToInt(cellCountX / (float)NUM_THREADS_X), Mathf.CeilToInt(cellCountY / (float)NUM_THREADS_Y), NUM_THREADS_Z);
                        Swap(_sootConcentration);
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

        public ComputeBuffer GetSootBuffer()
        {
            //return the read buffer as that contains the updated information after the swap
            return _sootConcentration[READ];
        }

        public void Release()
        {
            for (int i = 0; i < _allBuffers.Count; i++)
            {   
                if(_allBuffers[i] != null)
                {
                    _allBuffers[i].Release();
                    _allBuffers[i] = null;
                }                
            }
        }

        public override void Step(float currentTime, float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsSimulationDone()
        {
            return false;
        }

        public override float[] GetGroundSoot()
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}


