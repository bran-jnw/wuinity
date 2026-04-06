//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT.Input;
using PREACT.Visualization;
using PREACT.Math;
using PREACT;

namespace WUInity.Visualization
{
    public class FireRenderer : MonoBehaviour
    {
        [SerializeField] private Material _fireMaterial;
        [SerializeField] private Material sootMaterial;
        int _fireCellCountX, _fireCellCountY, _smokeCellCountX, _smokeCellCountY;

        ComputeBuffer _fireBuffer, sootBuffer;
        MeshRenderer fireMeshRenderer, sootMeshRenderer;
        float lowerExtCoeff = 0.002608695f; //500 meters with C = 3
        float upperExtCoeff = 0.260869565f; //5 meters with C = 3
        float lowerFirelineIntensityValue = 0.0f;
        float upperFirelineIntensityValue = 6000.0f;

        Texture2D horizontalRandomLegend;
        Texture2D fuelModelLegendTexture;


        public Material GetFireMaterial()
        {
            return _fireMaterial;
        }

        public Material GetSootMaterial()
        {
            return sootMaterial;
        }

        public bool ToggleFire(PREACTInput input)
        {
            if(input.WildfireModule.Enabled)
            {
                fireMeshRenderer.gameObject.SetActive(!fireMeshRenderer.gameObject.activeSelf);
                return fireMeshRenderer.gameObject.activeSelf;
            }
            else
            {
                return false;
            }
        }

        public bool ToggleSoot(PREACTInput input)
        {
            if(input.SmokeModule.Enabled)
            {
                sootMeshRenderer.gameObject.SetActive(!sootMeshRenderer.gameObject.activeSelf);
                return sootMeshRenderer.gameObject.activeSelf;
            }
            else
            {
                return false;
            }
        }

        public void CreateBuffers(Simulation simulation)
        {
            Release(simulation, true);

            if (simulation.Input.WildfireModule.Enabled)
            {
                CreateFireBuffer(simulation);
            }
            
            if(simulation.Input.SmokeModule.Enabled)
            {
                CreateSootBuffer(simulation);
            }            
        }

        private void CreateFireBuffer(Simulation simulation)
        {
            _fireCellCountX = simulation.Hazards.Wildfire.GetCellCountX();
            _fireCellCountY = simulation.Hazards.Wildfire.GetCellCountY();
            _fireBuffer = new ComputeBuffer(_fireCellCountX * _fireCellCountY, sizeof(float));
            _fireMaterial.SetInteger("_CellsX", _fireCellCountX);
            _fireMaterial.SetInteger("_CellsY", _fireCellCountY);
            SetFireDisplayMode(FireDisplayMode.FirelineIntensity);
            fireMeshRenderer = CreateDataPlane(_fireMaterial, "FireSpread", true, simulation);
        }

        private void CreateSootBuffer(Simulation simulation)
        {
            if (simulation.Input.SmokeModule.Module != SmokeInput.SmokeModules.None)
            {
                _smokeCellCountX = simulation.Hazards.Smoke.GetCellsX();
                _smokeCellCountY = simulation.Hazards.Smoke.GetCellsY();
                sootBuffer = new ComputeBuffer(_smokeCellCountX * _smokeCellCountY, sizeof(float));
                sootMaterial.SetInteger("_CellsX", _smokeCellCountX);
                sootMaterial.SetInteger("_CellsY", _smokeCellCountY);
                sootMaterial.SetFloat("_LowerCutOff", 0.0f);
                sootMaterial.SetFloat("_MinValue", lowerExtCoeff); //500 meters with C = 3
                sootMaterial.SetFloat("_MaxValue", upperExtCoeff); //5 meters with C = 3

                sootMeshRenderer = CreateDataPlane(sootMaterial, "SootSpread", true, simulation);

                if (simulation.Input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuseMixingLayer)
                {
                    // arrives in soot density, * 8700.0 (kg/m2, mass specific ext. coeff.) for extinction coefficient
                    sootMaterial.SetFloat("_DataMultiplier", 8700f); 
                }
                else
                {
                    sootMaterial.SetFloat("_DataMultiplier", 1f); // getting extinction coefficient directly
                }
            }
            else
            {
                Engine.Message(null, Engine.LogType.Warning, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
            }
        }       
        
        public enum FireDisplayMode { FirelineIntensity, FuelModelNumber, TimeOfArrival, DistanceToFront }
        FireDisplayMode _fireDisplayMode = FireDisplayMode.FirelineIntensity;

        public void SetFireDisplayMode(FireDisplayMode mode)
        {
            if(_fireMaterial == null)
            {
                return;
            }

            _fireDisplayMode = mode;
            if(_fireDisplayMode == FireDisplayMode.FirelineIntensity)
            {
                _fireMaterial.SetFloat("_LowerCutOff", 0.01f);
                _fireMaterial.SetFloat("_MinValue", lowerFirelineIntensityValue);
                _fireMaterial.SetFloat("_MaxValue", upperFirelineIntensityValue);
                _fireMaterial.SetFloat("_DataMultiplier", 1.0f);

                if(horizontalRandomLegend == null)
                {
                    horizontalRandomLegend = (Texture2D)_fireMaterial.GetTexture("_ScaleGradient");
                }
                _fireMaterial.SetTexture("_ScaleGradient", horizontalRandomLegend);
            }
            else if(_fireDisplayMode == FireDisplayMode.FuelModelNumber)
            {
                _fireMaterial.SetFloat("_LowerCutOff", 0.0f);
                _fireMaterial.SetFloat("_MinValue", 0);
                _fireMaterial.SetFloat("_MaxValue", 256);
                _fireMaterial.SetFloat("_DataMultiplier", 1.0f);

                if(fuelModelLegendTexture == null)
                {
                    CreateRandomFuelModelLegend();
                }
                _fireMaterial.SetTexture("_ScaleGradient", fuelModelLegendTexture);
            }
            else if(_fireDisplayMode == FireDisplayMode.TimeOfArrival)
            {
                _fireMaterial.SetFloat("_LowerCutOff", 0.01f);
                _fireMaterial.SetFloat("_MinValue", lowerFirelineIntensityValue);
                _fireMaterial.SetFloat("_MaxValue", upperFirelineIntensityValue);
                _fireMaterial.SetFloat("_DataMultiplier", 1.0f);
            }
            else if (_fireDisplayMode == FireDisplayMode.DistanceToFront)
            {
                _fireMaterial.SetFloat("_LowerCutOff", 0.0f);
                _fireMaterial.SetFloat("_MinValue", 0.0f);
                _fireMaterial.SetFloat("_MaxValue", 500.0f);
                _fireMaterial.SetFloat("_DataMultiplier", 1.0f);
            }
        }

        public void UpdateFireRenderer(bool renderFire, bool renderSoot, Simulation simulation)
        {
            if (renderFire)
            {
                float[] fireData = null;
                if (_fireDisplayMode == FireDisplayMode.FirelineIntensity)
                {
                    fireData = simulation.Hazards.Wildfire.GetFireLineIntensityData();
                }
                else if(_fireDisplayMode == FireDisplayMode.FuelModelNumber)
                {
                    fireData = simulation.Hazards.Wildfire.GetFuelModelNumberData();
                }
                
                if (fireData != null)
                {
                    _fireBuffer.SetData(fireData);
                    _fireMaterial.SetBuffer("_Data", _fireBuffer);
                }                
            }

            if (renderSoot)
            {
                if(simulation.Input.SmokeModule.Module != SmokeInput.SmokeModules.None)
                {
                    float[] newSoot = simulation.Hazards.Smoke.GetSootDensity();
                    if(newSoot != null)
                    {
                        sootBuffer.SetData(newSoot);
                        sootMaterial.SetBuffer("_Data", sootBuffer);
                    }                              
                }
                else
                {
                    Engine.Message(null, Engine.LogType.Warning, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
                }
            }
        }

        MeshRenderer CreateDataPlane(Material material, string name, bool setActive, Simulation simulation)
        {
            GameObject gO = new GameObject(name);
            gO.transform.parent = transform;
            gO.isStatic = true;
            // You can change that line to provide another MeshFilter
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh(); // filter.mesh;
            filter.mesh = mesh;
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mR.receiveShadows = false;
            mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mesh.Clear();

            float width;
            float height;
            Vector3 offset;
            Vector2 maxUV = Vector2.one;

            simulation.Hazards.Wildfire.GetOffsetAndSize(out Vector2d offsetFire, out Vector2d size);
            width = (float)size.x;
            height = (float)size.y;             
            offset = new Vector3((float)offsetFire.x, 0f, (float)offsetFire.y);

            VisualizeUtilities.CreateSimplePlane(mesh, width, height, 0.0f, offset);

            mR.material = material;
            //move up one meter
            gO.transform.position += Vector3.up;
            gO.SetActive(setActive);
            return mR;
        }

        void CreateRandomFuelModelLegend()
        {
            fuelModelLegendTexture = new Texture2D(256, 2);
            fuelModelLegendTexture.filterMode = FilterMode.Point;
            for (int i = 0; i < 256; i++)
            {
                PREACTColor fuelColor = FuelModelColors.GetFuelColor(i);
                fuelModelLegendTexture.SetPixel(i, 0, fuelColor.UnityColor());
                fuelModelLegendTexture.SetPixel(i, 1, fuelColor.UnityColor());
            }
            fuelModelLegendTexture.Apply();
        }

        public float GetUpperFirelineIntensityLimit()
        {
            return upperFirelineIntensityValue;
        }

        public void SetUpperFirelineIntensityLimit(float value)
        {
            _fireMaterial.SetFloat("_MaxValue", upperFirelineIntensityValue);
        }

        public float GetLowerFirelineIntensityLimit()
        {
            return lowerFirelineIntensityValue;
        }

        public void SetLowerFirelineIntensityLimit(float value)
        {
            _fireMaterial.SetFloat("_MinValue", lowerFirelineIntensityValue);
        }

        public float GetUpperExtCoeff()
        {
            return upperExtCoeff;
        }

        public void SetUpperOpticalDensityLimit(float value)
        {
            sootMaterial.SetFloat("_MaxValue", value); //5 meters with C = 3
        }

        public float GetLowerExtCoeff()
        {
            return lowerExtCoeff;
        }

        public void SetLowerOpticalDensityLimit(float value)
        {
            sootMaterial.SetFloat("_MinValue", value); //5 meters with C = 3
        }

        void OnDisable()
        {
            SetFireDisplayMode(FireDisplayMode.FirelineIntensity);
            Release();          
        }

        void OnDestroy()
        {
            Release();
        }

        void Release(Simulation simulation = null, bool creationCall = false)
        {
            if (_fireBuffer != null)
            {
                _fireBuffer.Release();
                _fireBuffer = null;
            }

            if (sootBuffer != null)
            {
                sootBuffer.Release();
                sootBuffer = null;
            }

            if(!creationCall && simulation != null && simulation.Hazards.Smoke != null)
            {
                if (simulation.Input.SmokeModule.Module == SmokeInput.SmokeModules.AdvectDiffuseMixingLayer)
                {
                    /*AdvectDiffuseModel model = simulation.SmokeModule as AdvectDiffuseModel;
                    if(model != null)
                    {
                        model.Release();
                    }*/
                }
                else
                {
                    Engine.Message(null, Engine.LogType.SimulationError, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
                }

                
            }
            
        }
    }    
}

