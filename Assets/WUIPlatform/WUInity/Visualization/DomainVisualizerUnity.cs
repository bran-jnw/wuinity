//This file is part of WUIPlatform Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT.Utility.Math;

namespace WUInity.Visualization
{
    public static class DomainVisualizerUnity
    {
        public static MeshRenderer CreateDomainPlane(GameObject gameObject, MeshRenderer renderer, Vector2d size, Vector2d originOffset)
        {            
            MeshFilter filter;
            Mesh mesh;
            MeshRenderer mR;

            if (renderer == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
                mR = gameObject.AddComponent<MeshRenderer>();
                mR.receiveShadows = false;
                mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mesh = new Mesh(); // filter.mesh;
                filter.mesh = mesh;
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                mR.material = mat;
            }
            else
            {
                filter = gameObject.GetComponent<MeshFilter>();
                mesh = filter.mesh;
                mR = renderer;
            }

            float width = (float)size.x;
            float length = (float)size.y;
            Vector3 offset = new Vector3((float)originOffset.x, 0.0f, (float)originOffset.y);
            mesh.Clear();
            VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset);

            return mR;
        }

        public static bool NeedNewPlane(Vector2d oldSize, Vector2d newSize, Vector2d oldLatLon, Vector2d newLatLon)
        {
            return Vector2d.SqrMagnitude(oldSize - newSize) > double.Epsilon && Vector2d.SqrMagnitude(oldLatLon - newLatLon) > double.Epsilon;
        }

        public static bool NeedNewTexture(Vector2int newDim, Texture2D oldTexture)
        {
            if(oldTexture == null)
            {
                return true;
            }

            return newDim.x != oldTexture.width || newDim.y != oldTexture.height;
        }
    }
}

