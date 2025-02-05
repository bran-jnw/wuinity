//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using WUIPlatform.Runtime;

namespace WUIPlatform.Visualization
{
    public class FireDataVisualizerUnity : FireDataVisualizer
    {
        GameObject _lcpDataPlane;
        Texture2D _fuelModelsTexture, _elevationTexture, _slopeTexture, _aspectTexture, _triggerBufferTexture;
        MeshRenderer lcpMeshRenderer;

        public FireDataVisualizerUnity(FireData owner) : base(owner) 
        {
            
        }

        public override void SetLCPViewMode(LcpViewMode lcpViewMode)
        {
            if (_lcpDataPlane == null)
            {
                CreateLCPVisuals();
            }

            if (lcpViewMode == LcpViewMode.FuelModel)
            {
                lcpMeshRenderer.material.mainTexture = _fuelModelsTexture;
            }
            else if (lcpViewMode == LcpViewMode.Elevation)
            {
                lcpMeshRenderer.material.mainTexture = _elevationTexture;
            }
            else if (lcpViewMode == LcpViewMode.Slope)
            {
                lcpMeshRenderer.material.mainTexture = _slopeTexture;
            }
            else if (lcpViewMode == LcpViewMode.Aspect)
            {
                lcpMeshRenderer.material.mainTexture = _aspectTexture;
            }
            else if (lcpViewMode == LcpViewMode.TriggerBuffer)
            {
                lcpMeshRenderer.material.mainTexture = _triggerBufferTexture;
            }
        }

        private void CreateLCPVisuals()
        {
            Fire.LCPData _lcpData = owner.LCPData;

            float xDim = (float)_lcpData.GetLCPSizeX();
            float yDim = (float)_lcpData.GetLCPSizeY();

            int xPixels = _lcpData.GetCellCountX();
            int yPixels = _lcpData.GetCellCountY();

            _fuelModelsTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _fuelModelsTexture.filterMode = FilterMode.Point;

            _elevationTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _elevationTexture.filterMode = FilterMode.Point;

            _slopeTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _slopeTexture.filterMode = FilterMode.Point;

            _aspectTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _aspectTexture.filterMode = FilterMode.Point;           

            Vector2d elevationMinMax = _lcpData.GetElevationMinMax();
            Vector2d slopeMinMax = _lcpData.GetSlopeMinMax();
            Vector2d aspectMinMax = _lcpData.GetAspectMinMax();

            float elevationRange = (float)(elevationMinMax.y - elevationMinMax.x);
            float slopeRange = (float)(slopeMinMax.y - slopeMinMax.x);
            float aspectRange = (float)(aspectMinMax.y - aspectMinMax.x);
            float alpha = 0.85f;

            for (int y = 0; y < yPixels; y++)
            {
                for (int x = 0; x < xPixels; x++)
                {
                    Fire.LandscapeStruct l = _lcpData.GetCellDataSimulationIndex(x, y, false);

                    WUIEngineColor c = FuelModelColors.GetFuelColor((int)l.fuel_model);
                    c.a = alpha;
                    _fuelModelsTexture.SetPixel(x, y, c.UnityColor);

                    c = WUIEngineColor.white * ((l.elevation - (float)elevationMinMax.x) / elevationRange);
                    c.a = alpha;
                    _elevationTexture.SetPixel(x, y, c.UnityColor);

                    c = WUIEngineColor.white * ((l.slope - (float)slopeMinMax.x) / slopeRange);
                    c.a = alpha;
                    _slopeTexture.SetPixel(x, y, c.UnityColor);

                    c = WUIEngineColor.white * ((l.aspect - (float)aspectMinMax.x) / aspectRange);
                    c.a = alpha;
                    _aspectTexture.SetPixel(x, y, c.UnityColor);
                }
            }

            _fuelModelsTexture.Apply();
            _elevationTexture.Apply();
            _slopeTexture.Apply();
            _aspectTexture.Apply();

            CreateLCPDataPlane(WUInity.WUInityEngine.INSTANCE.transform, "LCP_plane", true, xDim, yDim, _lcpData.OriginOffset);
        }

        public override void CreateTriggerBufferVisuals(float[,] triggerBufferData)
        {
            Fire.LCPData _lcpData = owner.LCPData;
            int xPixels = _lcpData.GetCellCountX();
            int yPixels = _lcpData.GetCellCountY();

            _triggerBufferTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
            _triggerBufferTexture.filterMode = FilterMode.Point;

            for (int y = 0; y < yPixels; y++)
            {
                for (int x = 0; x < xPixels; x++)
                {
                    //PERIL has  flipped x/y for some reason
                    float value = triggerBufferData[triggerBufferData.GetLength(0) - 1 - y, x];
                    Color c = Color.red * value;
                    c.a = 1.0f;
                    if(value == 0f)
                    {
                        c.a = 0f;
                    }
                    _triggerBufferTexture.SetPixel(x, y, c);
                }
            }

            _triggerBufferTexture.Apply();
        }        

        private MeshRenderer CreateLCPDataPlane(Transform parent, string name, bool setActive, float width, float length, Vector2d offset)
        {
            Mesh mesh;

            if (_lcpDataPlane == null)
            {
                _lcpDataPlane = new GameObject(name);
                _lcpDataPlane.transform.parent = parent;
                _lcpDataPlane.transform.position += Vector3.up;
                _lcpDataPlane.isStatic = true;

                MeshFilter filter = _lcpDataPlane.AddComponent<MeshFilter>();
                mesh = new Mesh(); // filter.mesh;
                filter.mesh = mesh;
                lcpMeshRenderer = _lcpDataPlane.AddComponent<MeshRenderer>();
                lcpMeshRenderer.receiveShadows = false;
                lcpMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mesh.Clear();
                lcpMeshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
            }
            else
            {
                mesh = _lcpDataPlane.GetComponent<MeshFilter>().mesh;
                mesh.Clear();
                lcpMeshRenderer = _lcpDataPlane.GetComponent<MeshRenderer>();
            }
            Vector3 unityOffset = new Vector3((float)offset.x, 0f, (float)offset.y);
            Vector2 maxUV = Vector2.one;

            WUInity.Visualization.VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, unityOffset, maxUV);

            lcpMeshRenderer.material.mainTexture = _fuelModelsTexture;

            _lcpDataPlane.SetActive(setActive);
            return lcpMeshRenderer;
        }

        public override void ToggleLCPDataPlane()
        {
            if (_lcpDataPlane == null)
            {
                CreateLCPVisuals();
            }

            _lcpDataPlane.SetActive(!_lcpDataPlane.activeSelf);
        }

        public override void SetLCPDataPlane(bool setActive)
        {
            if (_lcpDataPlane == null)
            {
                CreateLCPVisuals();
            }

            _lcpDataPlane.SetActive(setActive);
        }
    }
}

