using UnityEngine;
using WUInity.Fire;
using WUInity.Smoke;

namespace WUInity.Visualization
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
            if(WUInity.INPUT.Simulation.RunFireModule)
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
            if(WUInity.INPUT.Simulation.RunSmokeModule)
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
            fireCellCountX = WUInity.SIM.FireModule.GetCellCountX();
            fireCellCountY = WUInity.SIM.FireModule.GetCellCountY();
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
            //sootCellCountX = WUInity.SIM.GetSmokeDispersion().GetCellsX();
            //sootCellCountY = WUInity.SIM.GetSmokeDispersion().GetCellsY();
            if(WUInity.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
            {
                sootCellCountX = ((AdvectDiffuseModel)WUInity.SIM.SmokeModule).GetCellsX();
                sootCellCountY = ((AdvectDiffuseModel)WUInity.SIM.SmokeModule).GetCellsY();
                //sootBuffer = new ComputeBuffer(sootCellCountX * sootCellCountY, sizeof(float));
                sootMaterial.SetInteger("_CellsX", sootCellCountX);
                sootMaterial.SetInteger("_CellsY", sootCellCountY);
                sootMaterial.SetFloat("_LowerCutOff", 0.0f);
                sootMaterial.SetFloat("_MinValue", lowerSootValue); //500 meters with C = 3
                sootMaterial.SetFloat("_MaxValue", upperSootValue); //5 meters with C = 3
                sootMaterial.SetFloat("_DataMultiplier", 4539.13f); // 1.2 * 8700.0 / 2.3 = 4539.13 for optical density
                if (sootMeshRenderer == null)
                {
                    sootMeshRenderer = CreateDataPlane(sootMaterial, "SootSpread", true);
                }
            }
            else
            {
                WUInity.LOG(WUInity.LogType.Error, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
            }
                      
        }       
        
        public enum FireDisplayMode { FirelineIntensity, FuelModelNumber, TimeOfArrival }
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
                    fireData = WUInity.SIM.FireModule.GetFireLineIntensityData();
                }
                else if(_fireDisplayMode == FireDisplayMode.FuelModelNumber)
                {
                    fireData = WUInity.SIM.FireModule.GetFuelModelNumberData();
                }
                
                if (fireData != null)
                {
                    fireBuffer.SetData(fireData);
                    fireMaterial.SetBuffer("_Data", fireBuffer);
                }                
            }

            if (renderSoot)
            {
                if(WUInity.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
                {
                    ComputeBuffer buffer = ((AdvectDiffuseModel)WUInity.SIM.SmokeModule).GetSootBuffer();
                    if (buffer != null)
                    {
                        sootMaterial.SetBuffer("_Data", buffer);
                    }                    
                }
                else
                {
                    WUInity.LOG(WUInity.LogType.Error, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
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

            float width = (float)WUInity.INPUT.Simulation.Size.x;
            float height = (float)WUInity.INPUT.Simulation.Size.y;
            Vector3 offset = Vector3.zero;
            Vector2 maxUV = Vector2.one;

            if(material == fireMaterial && WUInity.INPUT.Fire.fireModuleChoice == FireInput.FireModuleChoice.FarsiteOffline)
            {
                float xScale, yScale;
                Vector2d offsetFire;
                ((FarsiteOffline)WUInity.SIM.FireModule).GetOffsetAndScale(out offsetFire, out xScale, out yScale);
                offset.x += (float)offsetFire.x;
                offset.y += (float)offsetFire.y;
                width *= xScale;
                height *= yScale;
            }

            VisualizeUtilities.CreateSimplePlane(mesh, width, height, 0.0f, offset, maxUV);

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
                WUInityColor fuelColor = FuelModelColors.GetFuelColor(i);
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

            if(!creationCall && WUInity.SIM.SmokeModule != null)
            {
                if (WUInity.INPUT.Smoke.smokeModuleChoice == SmokeInput.SmokeModuleChoice.AdvectDiffuse)
                {
                    ((AdvectDiffuseModel)WUInity.SIM.SmokeModule).Release();
                }
                else
                {
                    WUInity.LOG(WUInity.LogType.Error, "Unsupported smoke module, fire/smoke renderer failed to initialize.");
                }

                
            }
            
        }
    }    
}

