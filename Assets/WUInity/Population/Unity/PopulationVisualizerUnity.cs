using UnityEngine;
using WUIEngine.Runtime;

namespace WUIEngine.Population
{
    public class PopulationVisualizerUnity : PopulationVisualizer
    {
        private GameObject m_LocalGPWDataPlane;
        private Material gpwDataPlaneMaterial;

        public Texture2D populationTexture;
        public Texture2D densityTexture;

        public PopulationVisualizerUnity(PopulationManager owner) : base(owner)
        { 

        }

        public override bool IsDataPlaneActive()
        {
            return DataPlane.activeSelf;
        }

        public override object GetPopulationTexture()
        {
            return populationTexture;
        }

        public GameObject DataPlane
        {
            get
            {
                if (m_LocalGPWDataPlane == null)
                {
                    CreateLocalGPWDataPlane();
                }

                return m_LocalGPWDataPlane;
            }
        }

        public override void SetDataPlane(bool activeSelf)
        {
            DataPlane.SetActive(activeSelf);
        }

        public override bool ToggleLocalGPWVisibility()
        {
            DataPlane.SetActive(!m_LocalGPWDataPlane.activeSelf);

            return DataPlane.activeSelf;
        }

        public override void CreateTexture()
        {
            PopulationData data = owner.GetPopulationData();
            //first find the correct texture size
            int maxSide = Mathf.Max(data.cells.x, data.cells.y);
            Vector2int res = new Vector2int(2, 2);

            while (data.cells.x > res.x)
            {
                res.x *= 2;
            }
            while (owner.GetPopulationData().cells.y > res.y)
            {
                res.y *= 2;
            }

            populationTexture = new Texture2D(res.x, res.y);
            populationTexture.filterMode = FilterMode.Point;
            for (int y = 0; y < data.cells.y; y++)
            {
                for (int x = 0; x < data.cells.x; x++)
                {
                    double density = data.GetPeopleCount(x, y) / data.cellArea;
                    WUIEngineColor color = PopulationManager.GetGPWColor((float)density);

                    populationTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            populationTexture.Apply();
        }

        private void CreateLocalGPWDataPlane()
        {
            Mesh mesh;
            MeshRenderer mR;
            MeshFilter filter;

            if (m_LocalGPWDataPlane == null)
            {
                m_LocalGPWDataPlane = new GameObject("GPWDensityMap");
                m_LocalGPWDataPlane.transform.parent = WUInity.WUInity.INSTANCE.transform;
                m_LocalGPWDataPlane.isStatic = true;
                // You can change that line to provide another MeshFilter
                filter = m_LocalGPWDataPlane.AddComponent<MeshFilter>();
                mesh = new Mesh(); // filter.mesh;
                filter.mesh = mesh;
                mR = m_LocalGPWDataPlane.AddComponent<MeshRenderer>();
                mR.receiveShadows = false;
                mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                mR = m_LocalGPWDataPlane.GetComponent<MeshRenderer>();
                mesh = m_LocalGPWDataPlane.GetComponent<MeshFilter>().mesh;
                filter = m_LocalGPWDataPlane.GetComponent<MeshFilter>();
            }

            mesh.Clear();

            LocalGPWData localGPWData = owner.GetLocalGPWData();

            float width = (float)localGPWData.realWorldSize.x; //(float)size.x;
            float length = (float)localGPWData.realWorldSize.y; //(float)size.y;

            Vector3 offset = new Vector3((float)localGPWData.unityOriginOffset.x, 0.0f, (float)localGPWData.unityOriginOffset.y);

            Vector2 maxUV = new Vector2((float)localGPWData.dataSize.x / densityTexture.width, (float)localGPWData.dataSize.y / densityTexture.height);
            WUInity.Visualization.VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            if (gpwDataPlaneMaterial == null)
            {
                gpwDataPlaneMaterial = new Material(Shader.Find("Unlit/Transparent"));
            }
            gpwDataPlaneMaterial.mainTexture = densityTexture;

            mR.material = gpwDataPlaneMaterial;
            //filter.mesh = mesh;

            //move up one meter
            m_LocalGPWDataPlane.transform.position = Vector3.up;
            m_LocalGPWDataPlane.SetActive(false);
        }

        public override void CreateGPWTexture()
        {
            Vector2int dataSize = owner.GetLocalGPWData().dataSize;
            //first find the correct texture size
            int maxSide = Mathf.Max(dataSize.x, dataSize.y);
            Vector2int res = new Vector2int(2, 2);

            while (dataSize.x > res.x)
            {
                res.x *= 2;
            }
            while (dataSize.y > res.y)
            {
                res.y *= 2;
            }
            //Debug.Log("GPW texture resolution: " + res.x + ", " + res.y);

            //paint texture based time of arrival
            densityTexture = new UnityEngine.Texture2D(res.x, res.y);
            densityTexture.filterMode = UnityEngine.FilterMode.Point;
            for (int y = 0; y < dataSize.y; y++)
            {
                for (int x = 0; x < dataSize.x; x++)
                {
                    double density = owner.GetLocalGPWData().GetDensity(x, y);
                    WUIEngineColor color = PopulationManager.GetGPWColor((float)density);

                    densityTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            densityTexture.Apply();
        }
    }
}