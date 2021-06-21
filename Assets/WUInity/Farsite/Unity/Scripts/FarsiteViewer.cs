using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.Fire.MathWrap;

namespace WUInity.Farsite
{
    public class FarsiteViewer : MonoBehaviour
    {
        public float actualTime = 0.0f;
        [SerializeField] int terrainRes = 128;
        [SerializeField] Material tOAMAterial;
        public double timeScale = 1.0;
        [SerializeField] int fireRingRes = 32;
        [SerializeField] float burntUpdateTime = 0.5f;

        [SerializeField] Renderer burnt;
        [SerializeField] Renderer wind;

        [SerializeField] double fireGridSize = 1.0;
        const int arrayCount = 1024;
        int[,] fireGrid = new int[arrayCount, arrayCount];

        Texture2D burntTex;
        Texture2D windTex;
        float timer = 0.0f;
        [SerializeField] FarsiteData farsiteData;
        List<Terrain> terrains;
        Texture2D timeOfArrivalTex;
        Texture2D fuelModelTex;
        Texture2D firelineIntensityTex;
        GameObject terrainGO;

        public void ImportFarsite()
        {
            //fix fire numbers
            FarsiteFire[] fF = FindObjectsOfType<FarsiteFire>();
            for (int i = 0; i < fF.Length; ++i)
            {
                fF[i].fireNumber = i + 1;
                fF[i].resolution = fireRingRes;
            }

            farsiteData = new FarsiteData(Application.dataPath + "/Resources/_input/Farsite/");
            
            CreateRasterDataTextures();
            //CreateFarsiteTerrain();
            CreateCustomFarsiteTerrain();
            //DrawFirePerimeters();
            //CreateTOAPlane();
        }

        public void SetTime(float normalizedTime)
        {
            tOAMAterial.SetFloat("_normalizedMask", normalizedTime);
            actualTime = normalizedTime * (float)farsiteData.maxTimeOfArrivalRaster;
        }

        //https://en.wikipedia.org/wiki/Lambert_conformal_conic_projection
        public void TransformCoordinates()
        {
            //rox
            Vector2D latLong = new Vector2D(39.4027342779, -105.117169657);//new Vector2D(39.409924, -105.104505);

            double lambda = latLong.y * PI / 180.0; //actual long
            double phi = latLong.x * PI / 180.0; //actual lat

            double lambda_0 = -96.00 * PI / 180.0; //ref long
            double phi_0 = 23.00 * PI / 180.0; //ref lat
            double phi_1 = 29.50 * PI / 180.0; //parallel 1
            double phi_2 = 45.50 * PI / 180.0; //parallel 1

            double n = ln(cos(phi_1) * sec(phi_2)) / ln(tan(0.25 * PI + 0.5 * phi_2) * cot(0.25 * PI + 0.5 * phi_1));
            double F = cos(phi_1) * pow(tan(0.25 * PI + 0.5 * phi_1), n) / n;
            double rho = F * pow(cot(0.25 * PI + 0.5 * phi), n);
            double rho_0 = F * pow(cot(0.25 * PI + 0.5 * phi_0), n);

            double x = rho * sin(n * (lambda - lambda_0));
            double y = rho_0 - rho * cos(n * (lambda - lambda_0));
            //https://en.wikipedia.org/wiki/North_American_Datum
            double nad = 6378137.0;
            x *= nad;
            y *= nad;

            /*print(n);
            print(F);
            print(rho);
            print(rho_0);
            print("X: " + x + ", Y:" + y);
            print(y / -x);*/

            Vector2D v = global::WUInity.Conversions.LatLonToMeters(latLong);
            //print("X: " + v.x + ", Y:" + v.y);
        }

        private void CreatePlane(Mesh mesh, int resX, int resZ, float sizeX, float sizeZ, float yPos, Vector2 maxUV, bool getElevation)
        {
            resX++;
            resZ++;
            Vector3[] vertices = new Vector3[resX * resZ];
            for (int z = 0; z < resZ; z++)
            {
                float zPos = ((float)z / (resZ - 1)) * sizeZ;
                for (int x = 0; x < resX; x++)
                {
                    float xPos = ((float)x / (resX - 1)) * sizeX;
                    if(getElevation)
                    {
                        yPos = (float)farsiteData.GetElevationWorldSpace(xPos, zPos) - farsiteData.minElevation;
                        yPos = Mathf.Max(yPos, 0.0f);
                    }                    
                    vertices[x + z * resX] = new Vector3(xPos, yPos, zPos);
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

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        private void CreateRasterDataTextures()
        {
            //first find the correct texture size
            int maxSide = Mathf.Max(farsiteData.ncols, farsiteData.nrows);
            int res = 32;
            while (maxSide > res)
            {
                res *= 2;
            }
            //paint texture based time of arrival
            timeOfArrivalTex = new Texture2D(res, res);
            timeOfArrivalTex.filterMode = FilterMode.Point;
            for (int y = 0; y < farsiteData.nrows; y++)
            {
                for (int x = 0; x < farsiteData.ncols; x++)
                {
                    float tOA = farsiteData.GetTimeOfArrival(x, y);                    
                    Color color = Color.red * tOA / (float)farsiteData.maxTimeOfArrivalRaster;
                    color.a = 1.0f;
                    if (tOA < 0.0f)
                    {
                        color.a = 0.0f;
                    }
                    timeOfArrivalTex.SetPixel(x, y, color);
                }
            }
            timeOfArrivalTex.Apply();

            //paint texture based on fuel model
            fuelModelTex = new Texture2D(res, res);
            fuelModelTex.filterMode = FilterMode.Point;
            for (int y = 0; y < farsiteData.nrows; y++)
            {
                for (int x = 0; x < farsiteData.ncols; x++)
                {
                    float fuel = farsiteData.GetFuelModel(x, y);
                    Color color = Color.green * (0.1f + 0.9f * fuel);
                    if(fuel < 0.0f)
                    {
                        color = Color.grey;
                    }
                    fuelModelTex.SetPixel(x, y, color);
                }
            }
            fuelModelTex.Apply();

            //paint texture based on fuel model
            firelineIntensityTex = new Texture2D(res, res);
            firelineIntensityTex.filterMode = FilterMode.Point;
            for (int y = 0; y < farsiteData.nrows; y++)
            {
                for (int x = 0; x < farsiteData.ncols; x++)
                {
                    float f = farsiteData.GetFirelineIntensity(x, y);
                    f = (f - (float)farsiteData.minFireLineIntensity) / (float)(farsiteData.maxFireLineIntensity - farsiteData.minFireLineIntensity);
                    Color color = Color.HSVToRGB(0.67f - 0.67f * f, 1.0f, 1.0f);
                    firelineIntensityTex.SetPixel(x, y, color);
                }
            }
            firelineIntensityTex.Apply();
        }

        private void CreateCustomFarsiteTerrain()
        {
            terrainGO = new GameObject("CustomFarsiteTerrain");
            // You can change that line to provide another MeshFilter
            MeshFilter filter = terrainGO.AddComponent<MeshFilter>();
            Mesh mesh = filter.mesh;
            MeshRenderer mR = terrainGO.AddComponent<MeshRenderer>();
            mesh.Clear();

            float width = farsiteData.ncols * (float)farsiteData.cellsize;
            float length = farsiteData.nrows * (float)farsiteData.cellsize;

            //Unity has a hard limit if 65536 vertices on mesh unless setting mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 which takes more memory and bandwidth
            //CreatePlane(mesh, farsiteData.ncols + 1, farsiteData.ncols + 1, width, length);
            float xUV = farsiteData.ncols / (float)timeOfArrivalTex.width;
            float zUV = farsiteData.nrows / (float)timeOfArrivalTex.height;
            int xRes = terrainRes;
            int zRes = terrainRes;
            float ratio = farsiteData.nrows / (float)farsiteData.ncols;
            if (ratio < 1)
            {
                zRes = Mathf.RoundToInt(zRes * ratio); 
            }
            else
            {
                xRes = Mathf.RoundToInt(xRes / ratio);
            }
            CreatePlane(mesh, xRes, zRes, width, length, 0.0f, new Vector2(xUV, zUV), true);

            tOAMAterial.SetTexture("_MaskTex", timeOfArrivalTex);
            tOAMAterial.SetTexture("_DisplayTex", firelineIntensityTex);
            tOAMAterial.SetTexture("_DefaultTex", fuelModelTex);

            mR.material = tOAMAterial;
            terrainGO.transform.SetPositionAndRotation(new Vector3(16f, 0f, -495f), Quaternion.Euler(0f, -5.29f, 0f));
        }

        public void ToggleTerrain()
        {
            if(terrainGO != null)
            {
                terrainGO.SetActive(!terrainGO.activeSelf);
            }            
        }

        private void CreateTOAPlane()
        {
            GameObject gO = new GameObject("FarsiteTimeOfArrival");
            // You can change that line to provide another MeshFilter
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            Mesh mesh = filter.mesh;
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mesh.Clear();
                        
            float width = (float)farsiteData.cellsize * fuelModelTex.width;
            float length = (float)farsiteData.cellsize * fuelModelTex.height;

            float h = (float)(farsiteData.maxElevation - farsiteData.minElevation) + 10.0f;
            CreatePlane(mesh, 1, 1, width, length, h, Vector2.one, false);

            tOAMAterial.SetTexture("_MaskTex", timeOfArrivalTex);
            tOAMAterial.SetTexture("_DisplayTex", firelineIntensityTex);
            tOAMAterial.SetTexture("_DefaultTex", fuelModelTex);

            mR.material = tOAMAterial;
        }

        /// <summary>
        /// Generate Unity Terrain based on eleveation data from Farsite.
        /// </summary>
        private void CreateFarsiteTerrain()
        {
            if (farsiteData == null)
            {
                return;
            }

            GameObject gO = new GameObject("FarsiteTerrain");
            gO.isStatic = true;
            Terrain t = gO.AddComponent<Terrain>();
            TerrainData tD = new TerrainData();
            //find fitting resolution
            int maxSide = Mathf.Max(farsiteData.ncols, farsiteData.nrows);
            int res = Mathf.Max(fuelModelTex.width, fuelModelTex.height);
            //TODO: if res is bigger than 4096 we must create several terrrain objects and stitch them
            //Unity wants number based on 2 + 1
            tD.heightmapResolution = res + 1;
            tD.alphamapResolution = res;
            tD.SetHeights(0, 0, farsiteData.GetElevationUnityTerrain());
            float height = (float)(farsiteData.maxElevation - farsiteData.minElevation);
            float wh = (float)farsiteData.cellsize * (res);
            tD.size = new Vector3(wh, height, wh);
            t.terrainData = tD;

            SplatPrototype[] tex = new SplatPrototype[1];
            for (int i = 0; i < 1; i++)
            {
                tex[i] = new SplatPrototype();
                tex[i].texture = fuelModelTex; //timeOfArrivalTex;// 
                tex[i].tileSize = new Vector2(wh, wh);    //Sets the size of the texture
            }
            tD.splatPrototypes = tex;

            
            t.materialType = Terrain.MaterialType.BuiltInLegacyDiffuse;  

            float[,,] map = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, 1];
            for (int y = 0; y < t.terrainData.alphamapHeight; ++y)
            {
                for (int x = 0; x < t.terrainData.alphamapWidth; ++x)
                {
                    map[x, y, 0] = 1.0f;
                }
            }
            tD.SetAlphamaps(0, 0, map);
            
            t.Flush();

            terrains = new List<Terrain>();
            terrains.Add(t);
        }

        /// <summary>
        /// Quick and dirty drawing of the vector data using line renderers.
        /// </summary>
        private void DrawFirePerimeters()
        {
            if(farsiteData == null)
            {
                return;
            }

            GameObject vectorRoot = new GameObject("FarsiteVectorData");
            float currentTimeStamp = -1.0f;
            GameObject parent = null;
            for (int i = 0; i < farsiteData.vectorData.firePolygons.Count; ++i)
            {
                FarsitePolygon f = farsiteData.vectorData.firePolygons[i];
                if (f.timeStamp != currentTimeStamp)
                {
                    currentTimeStamp = (float)f.timeStamp;
                    parent = new GameObject("Time(min) " + f.timeStamp);
                    parent.transform.parent = vectorRoot.transform;
                }
                string name = "Fire " + f.fireNumber;
                GameObject gO = new GameObject(name);
                gO.isStatic = true;
                gO.transform.parent = parent.transform;
                LineRenderer lR = gO.AddComponent<LineRenderer>();
                lR.loop = true;
                lR.useWorldSpace = true;
                lR.positionCount = f.vertices.Count;
                lR.startWidth = 10.0f;
                lR.endWidth = 10.0f;
                for (int j = 0; j < lR.positionCount; ++j)
                {
                    Vector3 pointPosition = (Vector3)f.vertices[j].pos;
                    //place on top of terrain, add 10 meters to avoid z-fighting
                    float h = (float)farsiteData.GetElevationWorldSpace(pointPosition.x, pointPosition.z) - farsiteData.minElevation + 10.0f;
                    h = Mathf.Max(0.0f, h);
                    pointPosition += Vector3.up * (h + 10.0f); 
                    lR.SetPosition(j, pointPosition);
                    //set color based on fireline intensity
                    //float fI = (float)f.vertices[j].firelineIntensity;
                    //fI = (fI - (float)farsiteData.minFireLineIntensity) / (float)(farsiteData.maxFireLineIntensity - farsiteData.minFireLineIntensity);
                    //Color color = Color.HSVToRGB(0.67f - 0.67f * fI, 1.0f, 1.0f);
                }

            }
        }

        public int CheckFireGrid(int fireNumber, double x, double y)
        {
            int xPos = (int)(x / fireGridSize);
            int yPos = (int)(y / fireGridSize);

            int g = 0;
            //check if inside grid
            if(xPos >= 0 && xPos < arrayCount && yPos >= 0 && yPos < arrayCount)
            {
                g = fireGrid[xPos, yPos];
                //if no fire present in cell mark with current fire
                if(g == 0)
                {
                    fireGrid[xPos, yPos] = fireNumber;
                    burntTex.SetPixel(xPos, yPos, Color.red);
                }
                /*else if(g != 0 && fireNumber != g)
                {
                    tex.SetPixel(xPos, yPos, Color.yellow);
                    tex.Apply();
                }*/
            }
            
            return g;
        }
    }
}
