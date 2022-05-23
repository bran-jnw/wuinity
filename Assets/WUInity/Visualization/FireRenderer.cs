using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.Visualization
{
    public class FireRenderer : MonoBehaviour
    {
        [SerializeField] private Material fireMaterial;
        [SerializeField] private Material sootMaterial;
        int fireCellCountX, fireCellCountY, sootCellCountX, sootCellCountY;

        ComputeBuffer fireBuffer, sootBuffer;
        MeshRenderer fireMeshRenderer, sootMeshRenderer;


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
            if(WUInity.INPUT.runFireSim)
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
            if(WUInity.INPUT.runSmokeSim)
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
            fireCellCountX = WUInity.SIM.FireMesh().cellCount.x;
            fireCellCountY = WUInity.SIM.FireMesh().cellCount.y;
            fireBuffer = new ComputeBuffer(fireCellCountX * fireCellCountY, sizeof(float));
            fireMaterial.SetInteger("_CellsX", fireCellCountX);
            fireMaterial.SetInteger("_CellsY", fireCellCountY);
            fireMaterial.SetFloat("_LowerCutOff", 0.01f);
            fireMaterial.SetFloat("_MinValue", 0.0f);
            fireMaterial.SetFloat("_MaxValue", 6000.0f);
            fireMaterial.SetFloat("_DataMultiplier", 1.0f);
            if (fireMeshRenderer == null)
            {
                fireMeshRenderer = CreateDataPlane(fireMaterial, "FireSpread", true);
            }            
        }

        void CreateSootBuffer()
        {
            //sootCellCountX = WUInity.SIM.GetSmokeDispersion().GetCellsX();
            //sootCellCountY = WUInity.SIM.GetSmokeDispersion().GetCellsY();
            sootCellCountX = WUInity.SIM.AdvectDiffuseSim().GetCellsX();
            sootCellCountY = WUInity.SIM.AdvectDiffuseSim().GetCellsY();
            //sootBuffer = new ComputeBuffer(sootCellCountX * sootCellCountY, sizeof(float));
            sootMaterial.SetInteger("_CellsX", sootCellCountX);
            sootMaterial.SetInteger("_CellsY", sootCellCountY);
            sootMaterial.SetFloat("_LowerCutOff", 0.0f);
            sootMaterial.SetFloat("_MinValue", 0.002608695f); //500 meters with C = 3
            sootMaterial.SetFloat("_MaxValue", 0.260869565f); //5 meters with C = 3
            sootMaterial.SetFloat("_DataMultiplier", 4539.13f); // 1.2 * 8700.0 / 2.3 = 4539.13 for optical density
            if (sootMeshRenderer == null)
            {
                sootMeshRenderer = CreateDataPlane(sootMaterial, "SootSpread", true);
            }            
        }        

        public void UpdateFireRenderer(bool renderFire, bool renderSoot)
        {
            if (renderFire)
            {
                float[] fireData = WUInity.SIM.FireMesh().GetFireLineIntensityData();
                if(fireData != null)
                {
                    fireBuffer.SetData(fireData);
                    fireMaterial.SetBuffer("_Data", fireBuffer);
                }                
            }

            if (renderSoot)
            {
                /*float[] sootData = WUInity.SIM.GetSmokeDispersion().GetData();
                if (sootData != null)
                {
                    sootBuffer.SetData(sootData);
                    sootMaterial.SetBuffer("_Data", sootBuffer);
                }  */
                ComputeBuffer buffer = WUInity.SIM.AdvectDiffuseSim().GetSootBuffer();
                sootMaterial.SetBuffer("_Data", buffer);
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

            float width = (float)WUInity.INPUT.size.x;
            float length = (float)WUInity.INPUT.size.y;
            Vector3 offset = Vector3.zero;
            Vector2 maxUV = Vector2.one;

           Population.PopulationManager.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            mR.material = material;
            //move up one meter
            gO.transform.position += Vector3.up;
            gO.SetActive(setActive);
            return mR;
        }

        void OnDisable()
        {
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

            if(!creationCall && WUInity.SIM.AdvectDiffuseSim() != null)
            {
                WUInity.SIM.AdvectDiffuseSim().Release();
            }
            
        }
    }    
}

