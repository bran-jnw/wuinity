//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;

namespace WUIPlatform.WUInity.Visualization
{
    public static class VisualizeUtilities
    {
        static MeshRenderer CreateDataPlane(Transform parent, Material material, string name, bool setActive)
        {
            GameObject gO = new GameObject(name);
            gO.transform.parent = parent;
            gO.isStatic = true;
            // You can change that line to provide another MeshFilter
            MeshFilter filter = gO.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh(); // filter.mesh;
            filter.mesh = mesh;
            MeshRenderer mR = gO.AddComponent<MeshRenderer>();
            mR.receiveShadows = false;
            mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mesh.Clear();

            float width = (float)WUIEngine.INPUT.Simulation.Size.x;
            float length = (float)WUIEngine.INPUT.Simulation.Size.y;
            Vector3 offset = Vector3.zero;
            Vector2 maxUV = Vector2.one;

            CreateSimplePlane(mesh, width, length, 0.0f, offset);

            mR.material = material;
            //move up one meter
            gO.transform.position += Vector3.up;
            gO.SetActive(setActive);
            return mR;
        }


        public static void CreateSimplePlane(Mesh mesh, float sizeX, float sizeZ, float yPos, Vector3 originOffset)
        {
            const int resX = 2;
            const int resZ = 2;

            Vector3[] vertices = new Vector3[resX * resZ];
            for (int z = 0; z < resZ; z++)
            {
                float zPos = ((float)z / (resZ - 1)) * sizeZ;
                for (int x = 0; x < resX; x++)
                {
                    float xPos = ((float)x / (resX - 1)) * sizeX;
                    vertices[x + z * resX] = new Vector3(xPos, yPos, zPos) + originOffset;
                }
            }
            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < normals.Length; n++)
            {
                normals[n] = Vector3.up;
            }
            Vector2[] uvs = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };

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
    }
}

