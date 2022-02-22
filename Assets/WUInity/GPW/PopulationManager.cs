using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.GPW
{

    public class PopulationManager
    {
        private LocalGPWData localGPWData;
        private PopulationData populationData;
        private GameObject internal_localGPWDataPlane;
        private Material gpwDataPlaneMaterial;

        public PopulationManager()
        {
            localGPWData = new LocalGPWData();
            populationData = new PopulationData();
        }

        public bool IsPopulationLoaded()
        {
            return populationData.isLoaded;
        }

        public bool IsLocalGPWLoaded()
        {
            return localGPWData.isLoaded;
        }

        public PopulationData GetPopulationData()
        {
            return populationData;
        }

        public LocalGPWData GetLocalGPWData()
        {
            return localGPWData;
        }

        public Texture2D GetPopulationTexture()
        {
            return populationData.GetPopulationTexture();
        }

        public int GetTotalPopulation()
        {
            return populationData.totalPopulation;
        }

        public int GetPopulation(int x, int y)
        {
            return populationData.cellPopulation[x + y * populationData.cells.x];
        }

        public int GetLocalGPWTotalPopulation()
        {
            return localGPWData.totalPopulation;
        }        

        public bool LoadPopulationFromFile()
        {
            return populationData.LoadPopulationFromFile();
        }

        public bool LoadLocalGPWFromFile()
        {
            return localGPWData.LoadLocalGPWDataFromFile();
        }

        public bool CreatePopulationFromLocalGPW()
        {            
            bool success = localGPWData.LoadLocalGPWDataFromFile();

            if(success)
            {
                DATA_PLANE.SetActive(false);
                populationData.CreatePopulationFromLocalGPW(localGPWData);
            }            

            return success;
        }

        public bool CreateLocalGPW()
        {
            return localGPWData.CreateLocalGPWData();
        }

        public GameObject DATA_PLANE
        {
            get
            {
                if (internal_localGPWDataPlane == null)
                {
                    CreateLocalGPWDataPlane();
                }

                return internal_localGPWDataPlane;
            }
        }

        public bool ToggleLocalGPWVisibility()
        {
            DATA_PLANE.SetActive(!internal_localGPWDataPlane.activeSelf);

            return DATA_PLANE.activeSelf;
        }  

        private void CreateLocalGPWDataPlane()
        {
            Mesh mesh;
            MeshRenderer mR;

            if(internal_localGPWDataPlane == null)
            {
                internal_localGPWDataPlane = new GameObject("GPWDensityMap");
                internal_localGPWDataPlane.transform.parent = WUInity.INSTANCE.transform;
                internal_localGPWDataPlane.isStatic = true;
                // You can change that line to provide another MeshFilter
                MeshFilter filter = internal_localGPWDataPlane.AddComponent<MeshFilter>();
                mesh = new Mesh(); // filter.mesh;
                filter.mesh = mesh;
                mR = internal_localGPWDataPlane.AddComponent<MeshRenderer>();
                mR.receiveShadows = false;
                mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                mR = internal_localGPWDataPlane.GetComponent<MeshRenderer>();
                mesh = internal_localGPWDataPlane.GetComponent<MeshFilter>().mesh;
            }
            
            mesh.Clear();

            float width = (float)localGPWData.realWorldSize.x; //(float)size.x;
            float length = (float)localGPWData.realWorldSize.y; //(float)size.y;

            Vector3 offset = new Vector3((float)localGPWData.unityOriginOffset.x, 0.0f, (float)localGPWData.unityOriginOffset.y);

            Vector2 maxUV = new Vector2((float)localGPWData.dataSize.x / localGPWData.densityTexture.width, (float)localGPWData.dataSize.y / localGPWData.densityTexture.height);
            CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            if(gpwDataPlaneMaterial == null)
            {
                gpwDataPlaneMaterial = new Material(Shader.Find("Unlit/Transparent"));
            }            
            gpwDataPlaneMaterial.mainTexture = localGPWData.densityTexture;

            mR.material = gpwDataPlaneMaterial;

            //move up one meter
            internal_localGPWDataPlane.transform.position = Vector3.up;
            internal_localGPWDataPlane.SetActive(false);
        }        

        public static void CreateSimplePlane(Mesh mesh, float sizeX, float sizeZ, float yPos, Vector3 offset, Vector2 maxUV)
        {
            int resX = 2;
            int resZ = 2;

            Vector3[] vertices = new Vector3[resX * resZ];
            for (int z = 0; z < resZ; z++)
            {
                float zPos = ((float)z / (resZ - 1)) * sizeZ;
                for (int x = 0; x < resX; x++)
                {
                    float xPos = ((float)x / (resX - 1)) * sizeX;
                    vertices[x + z * resX] = new Vector3(xPos, yPos, zPos) + offset;
                }
            }
            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < normals.Length; n++)
            {
                normals[n] = Vector3.up;
            }
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int v = 0; v < resZ; v++)
            {
                for (int u = 0; u < resX; u++)
                {
                    uvs[u + v * resX] = new Vector2((float)u / (resX - 1) * maxUV.x, (float)v / (resZ - 1) * maxUV.y);
                }
            }
            int nbFaces = (resX - 1) * (resZ - 1);
            int[] triangles = new int[nbFaces * 6];
            int index = 0;
            for (int y = 0; y < resZ - 1; y++)
            {
                for (int x = 0; x < resX - 1; x++)
                {
                    triangles[index] = (y * resX) + x;
                    triangles[index + 1] = ((y + 1) * resX) + x;
                    triangles[index + 2] = (y * resX) + x + 1;

                    triangles[index + 3] = ((y + 1) * resX) + x;
                    triangles[index + 4] = ((y + 1) * resX) + x + 1;
                    triangles[index + 5] = (y * resX) + x + 1;
                    index += 6;
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }    

        //colors from GPW website
        static Color c0 = new Color(190f / 255f, 232f / 255f, 255f / 255f);
        static Color c1 = new Color(1.0f, 241f / 255f, 208f / 255f);
        static Color c2 = new Color(1.0f, 218f / 255f, 165f / 255f);
        static Color c3 = new Color(252f / 255f, 183f / 255f, 82f / 255f);
        static Color c4 = new Color(1.0f, 137f / 255f, 63f / 255f);
        static Color c5 = new Color(238f / 255f, 60f / 255f, 30f / 255f);
        static Color c6 = new Color(191f / 255f, 1f / 255f, 39f / 255f);

        public static Color GetGPWColor(float density)
        {
            Color color;
            if (density < 0.0f)
            {
                color = c0;
            }
            else if (density < 1.0f)
            {
                color = c1;
            }
            else if (density <= 5.0f)
            {
                color = c2;
            }
            else if (density <= 25.0f)
            {
                color = c3;
            }
            else if (density <= 250.0f)
            {
                color = c4;
            }
            else if (density <= 1000.0f)
            {
                color = c5;
            }
            else
            {
                color = c6;
            }
            color.a = 0.7f;
            return color;
        }
    }
}