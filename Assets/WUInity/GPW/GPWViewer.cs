using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity.GPW
{

    public class GPWViewer
    {
        [SerializeField] Vector2D lowerLeftlatLong;
        [SerializeField] Vector2D size = new Vector2D(20000, 20000);

        [SerializeField] public GPWData gpwData;

        public  GameObject gpwDensityMap;
        Material mat;

        public bool CreateGPW(Vector2D lowerLeftLatLong, Vector2D mapSize, bool setActive)
        {
            if (gpwDensityMap != null)
            {
                MonoBehaviour.Destroy(gpwDensityMap);
            }
            size = mapSize;
            this.lowerLeftlatLong = lowerLeftLatLong;
            gpwData = new GPWData();
            bool success = gpwData.LoadGPWData(lowerLeftlatLong, size);

            CreateTexture();
            CreateDensityPlane();

            gpwDensityMap.SetActive(setActive);

            return success;
        }

        public void SetTexture(Texture2D tex)
        {
            mat.mainTexture = tex;
        }

        private void CreateDensityPlane()
        {
            GameObject gO = new GameObject("GPWDensityMap");
            gO.transform.parent = WUInity.INSTANCE.transform;
            gO.isStatic = true;
            // You can change that line to provide another MeshFilter
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh(); // filter.mesh;
            filter.mesh = mesh;
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mR.receiveShadows = false;
            mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mesh.Clear();

            float width = (float)gpwData.realWorldSize.x; //(float)size.x;
            float length = (float)gpwData.realWorldSize.y; //(float)size.y;
                                                           //Debug.Log("" + width + "," + length);


            Vector3 offset = new Vector3((float)gpwData.unityOriginOffset.x, 0.0f, (float)gpwData.unityOriginOffset.y);

            Vector2 maxUV = new Vector2((float)gpwData.dataSize.x / densityTex.width, (float)gpwData.dataSize.y / densityTex.height);
            CreateSimplePlane(mesh, width, length, 0.0f, offset, maxUV);

            mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = densityTex;

            mR.material = mat;

            //move up one meter
            gO.transform.position += Vector3.up;

            gpwDensityMap = gO;
        }

        public void ToggleDensityMapVisibility()
        {
            if(gpwDensityMap != null)
            {
                gpwDensityMap.SetActive(!gpwDensityMap.activeSelf);
            }            
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

        Texture2D densityTex;
        private void CreateTexture()
        {
            //first find the correct texture size
            int maxSide = Mathf.Max(gpwData.dataSize.x, gpwData.dataSize.y);
            Vector2Int res = new Vector2Int(2, 2);

            while (gpwData.dataSize.x > res.x)
            {
                res.x *= 2;
            }
            while (gpwData.dataSize.y > res.y)
            {
                res.y *= 2;
            }
            //Debug.Log("GPW texture resolution: " + res.x + ", " + res.y);

            //paint texture based time of arrival
            densityTex = new Texture2D(res.x, res.y);
            densityTex.filterMode = FilterMode.Point;
            for (int y = 0; y < gpwData.dataSize.y; y++)
            {
                for (int x = 0; x < gpwData.dataSize.x; x++)
                {
                    double density = gpwData.GetDensity(x, y);
                    Color color = GetGPWColor((float)density);

                    densityTex.SetPixel(x, y, color);
                }
            }
            densityTex.Apply();
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