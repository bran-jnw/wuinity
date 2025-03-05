//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using WUIPlatform.Fire;
using WUIPlatform.Smoke;
using WUIPlatform.IO;
using WUIPlatform.Visualization;

namespace WUIPlatform.WUInity.Visualization
{
    public class FireRenderer : MonoBehaviour
    {
        [SerializeField] private Material fireMaterial;
        [SerializeField] private Material sootMaterial;
        int fireCellCountX, fireCellCountY, sootCellCountX, sootCellCountY;

        ComputeBuffer fireBuffer, sootBuffer;
        MeshRenderer fireMeshRenderer, sootMeshRenderer;
        float lowerSootValue = 0.002608695f; //500 meters with C = 3
        float upperSootValue = 0.260869565f; //5 meters with C = 3
        float lowerFirelineIntensityValue = 0.0f;
        float upperFirelineIntensityValue = 6000.0f;

        Texture2D horizontalRandomLegend;
        Texture2D fuelModelLegendTexture;


        public Material GetFireMaterial()
        {
            return fireMaterial;
        }

        public Material GetSootMaterial()
        {
            return sootMaterial;
        }

        public bool ToggleFire()
        {
            if(WUIEngine.INPUT.Simulation.RunFireModule)
            {
                fireMeshRenderer.gameObject.SetActive(!fireMeshRenderer.gameObject.activeSelf);
                return fireMeshRenderer.gameObject.activeSelf;
            }
            else
            {
                return false;
            }
        }

        public bool ToggleSoot()
        {
            if(WUIEngine.INPUT.Simulation.RunSmokeModule)
            {
                sootMeshRenderer.gameObject.SetActive(!sootMeshRenderer.gameObject.activeSelf);
                return sootMeshRenderer.gameObject.activeSelf;
            }
            else
            {
                return false;
            }
        }

        public void CreateBuffers(bool renderFire, bool renderSmoke)
        {
            Release(true);

            if (renderFire)
            {
                CreateFireBuffer();
            }
            
            if(renderSmoke)
            {
                CreateSootBuffer();
            }            
        }

        void CreateFireBuffer()
        {            
            fireCellCountX = WUIEngine.SIM.FireModule.GetCellCountX();
            fireCellCountY = WUIEngine.SIM.FireModule.GetCellCountY();
            fireBuffer = new ComputeBuffer(fireCellCountX * fireCellCountY, sizeof(float));
            fireMaterial.SetInteger("_CellsX", fireCellCountX);
            fireMaterial.SetInteger("_CellsY", fireCellCountY);
            SetFireDisplayMode(FireDisplayMode.FirelineIntensity);
            if (fireMeshRenderer == null)
            {
                fireMeshRenderer = CreateDataPlane(fireMaterial, "FireSpread", true);
            }

            SetFireOffsetAndScale();
        }

        void CreateSootBuffer()
        {
            if(WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer
                || WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
            {
                sootCellCountX = WUIEngine.SIM.SmokeModule.GetCellsX();
                sootCellCountY = WUIEngine.SIM.SmokeModule.GetCellsY();
                sootBuffer = new ComputeBuffer(sootCellCountX * sootCellCountY, sizeof(float));
                sootMaterial.SetInteger("_CellsX", sootCellCountX);
                sootMaterial.SetInteger("_CellsY", sootCellCountY);
                sootMaterial.SetFloat("_LowerCutOff", 0.0f);
                sootMaterial.SetFloat("_MinValue", lowerSootValue); //500 meters with C = 3
                sootMaterial.SetFloat("_MaxValue", upperSootValue); //5 meters with C = 3

                if(WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer)
                {
                    // arrives in soot density, * 8700.0 for extinction coefficient
                    sootMaterial.SetFloat("_DataMultiplier", 8700f); 
                }
                else
                {
                    sootMaterial.SetFloat("_DataMultiplier", 1f); // getting exticntion coefficient directly
                }
                
                if (sootMeshRenderer == null)
                {
                    sootMeshRenderer = CreateDataPlane(sootMaterial, "SootSpread", true);
                }
            }
            else
            {
                WUIEngine.LOG(WUIEngine.LogType.Warning, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
            }
                      
        }       
        
        public enum FireDisplayMode { FirelineIntensity, FuelModelNumber, TimeOfArrival}
        FireDisplayMode _fireDisplayMode = FireDisplayMode.FirelineIntensity;

        public void SetFireDisplayMode(FireDisplayMode mode)
        {
            _fireDisplayMode = mode;
            if(_fireDisplayMode == FireDisplayMode.FirelineIntensity)
            {
                fireMaterial.SetFloat("_LowerCutOff", 0.01f);
                fireMaterial.SetFloat("_MinValue", lowerFirelineIntensityValue);
                fireMaterial.SetFloat("_MaxValue", upperFirelineIntensityValue);
                fireMaterial.SetFloat("_DataMultiplier", 1.0f);

                if(horizontalRandomLegend == null)
                {
                    horizontalRandomLegend = (Texture2D)fireMaterial.GetTexture("_ScaleGradient");
                }
                fireMaterial.SetTexture("_ScaleGradient", horizontalRandomLegend);
            }
            else if(_fireDisplayMode == FireDisplayMode.FuelModelNumber)
            {
                fireMaterial.SetFloat("_LowerCutOff", 0.0f);
                fireMaterial.SetFloat("_MinValue", 0);
                fireMaterial.SetFloat("_MaxValue", 256);
                fireMaterial.SetFloat("_DataMultiplier", 1.0f);

                if(fuelModelLegendTexture == null)
                {
                    CreateRandomFuelModelLegend();
                }
                fireMaterial.SetTexture("_ScaleGradient", fuelModelLegendTexture);
            }
            else if(_fireDisplayMode == FireDisplayMode.TimeOfArrival)
            {
                fireMaterial.SetFloat("_LowerCutOff", 0.01f);
                fireMaterial.SetFloat("_MinValue", lowerFirelineIntensityValue);
                fireMaterial.SetFloat("_MaxValue", upperFirelineIntensityValue);
                fireMaterial.SetFloat("_DataMultiplier", 1.0f);
            }
        }

        public void UpdateFireRenderer(bool renderFire, bool renderSoot)
        {
            if (renderFire)
            {
                float[] fireData = null;
                if (_fireDisplayMode == FireDisplayMode.FirelineIntensity)
                {
                    fireData = WUIEngine.SIM.FireModule.GetFireLineIntensityData();
                }
                else if(_fireDisplayMode == FireDisplayMode.FuelModelNumber)
                {
                    fireData = WUIEngine.SIM.FireModule.GetFuelModelNumberData();
                }
                
                if (fireData != null)
                {
                    fireBuffer.SetData(fireData);
                    fireMaterial.SetBuffer("_Data", fireBuffer);
                }                
            }

            if (renderSoot)
            {
                if(WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer
                    || WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.GlobalSmoke)
                {
                    float[] newSoot = WUIEngine.SIM.SmokeModule.GetExtinctionCoefficientDensity();
                    if(newSoot != null)
                    {
                        sootBuffer.SetData(newSoot);
                        sootMaterial.SetBuffer("_Data", sootBuffer);
                    }                              
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
                }
            }
        }

        MeshRenderer CreateDataPlane(Material material, string name, bool setActive)
        {
            GameObject gO = new GameObject(name);
            gO.transform.parent = this.transform;
            gO.isStatic = true;
            // You can change that line to provide another MeshFilter
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh(); // filter.mesh;
            filter.mesh = mesh;
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mR.receiveShadows = false;
            mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mesh.Clear();

            float width = (float)WUIEngine.INPUT.Simulation.DomainSize.x;
            float height = (float)WUIEngine.INPUT.Simulation.DomainSize.y;
            Vector3 offset = Vector3.zero;
            Vector2 maxUV = Vector2.one;

            if(WUIEngine.INPUT.Fire.FireModule == FireInput.FireModuleChoice.AscImport)
            {
                float xScale, yScale;
                Vector2d offsetFire;
                ((AscFireImport)WUIEngine.SIM.FireModule).GetOffsetAndScale(out offsetFire, out xScale, out yScale);
                offset.x += (float)offsetFire.x;
                offset.y += (float)offsetFire.y;
                width *= xScale;
                height *= yScale;
            }

            VisualizeUtilities.CreateSimplePlane(mesh, width, height, 0.0f, offset);

            mR.material = material;
            //move up one meter
            gO.transform.position += Vector3.up;
            gO.SetActive(setActive);
            return mR;
        }

        private void SetFireOffsetAndScale()
        {
            
        }

        void CreateRandomFuelModelLegend()
        {
            fuelModelLegendTexture = new Texture2D(256, 2);
            fuelModelLegendTexture.filterMode = FilterMode.Point;
            for (int i = 0; i < 256; i++)
            {
                WUIEngineColor fuelColor = FuelModelColors.GetFuelColor(i);
                fuelModelLegendTexture.SetPixel(i, 0, fuelColor.UnityColor);
                fuelModelLegendTexture.SetPixel(i, 1, fuelColor.UnityColor);
            }
            fuelModelLegendTexture.Apply();
        }

        public float GetUpperFirelineIntensityLimit()
        {
            return upperFirelineIntensityValue;
        }

        public void SetUpperFirelineIntensityLimit(float value)
        {
            fireMaterial.SetFloat("_MaxValue", upperFirelineIntensityValue);
        }

        public float GetLowerFirelineIntensityLimit()
        {
            return lowerFirelineIntensityValue;
        }

        public void SetLowerFirelineIntensityLimit(float value)
        {
            fireMaterial.SetFloat("_MinValue", lowerFirelineIntensityValue);
        }

        public float GetUpperOpticalDensityLimit()
        {
            return upperSootValue;
        }

        public void SetUpperOpticalDensityLimit(float value)
        {
            sootMaterial.SetFloat("_MaxValue", value); //5 meters with C = 3
        }

        public float GetLowerOpticalDensityLimit()
        {
            return lowerSootValue;
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

        void Release(bool creationCall = false)
        {
            if (fireBuffer != null)
            {
                fireBuffer.Release();
                fireBuffer = null;
            }

            if (sootBuffer != null)
            {
                sootBuffer.Release();
                sootBuffer = null;
            }

            if(!creationCall && WUIEngine.SIM.SmokeModule != null)
            {
                if (WUIEngine.INPUT.Smoke.SmokeModule == SmokeInput.SmokeModuleChoice.AdvectDiffuseMixingLayer)
                {
                    AdvectDiffuseModel model = WUIEngine.SIM.SmokeModule as AdvectDiffuseModel;
                    if(model != null)
                    {
                        model.Release();
                    }
                }
                else
                {
                    WUIEngine.LOG(WUIEngine.LogType.SimError, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
                }

                
            }
            
        }
    }    
}

