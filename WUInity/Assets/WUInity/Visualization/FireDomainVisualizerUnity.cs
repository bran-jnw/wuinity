//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT;
using PREACT.Wildfire;
using PREACT.Math;
using PREACT.Visualization;

namespace WUInity.Visualization
{
    public class FireDomainVisualizerUnity : FireDomainVisualizer
    {
        private GameObject _lcpDomainPlane;
        MeshRenderer _lcpDomainMeshRenderer;
        LandscapeData _lcpData;
        //textures
        Texture2D _fuelModelsTexture, _elevationTexture, _slopeTexture, _aspectTexture, _triggerBufferTexture;
          

        public FireDomainVisualizerUnity(Transform parent)
        {
            _lcpDomainPlane = new GameObject("WildfireDomain");
            _lcpDomainPlane.transform.parent = parent;
            _lcpDomainPlane.transform.position += Vector3.up;
            _lcpDomainPlane.isStatic = true;            
        }

        public void SetLCPPlaneTexture(Texture2D tex)
        {
            if (_lcpData == null)
            {
                return;
            }

            bool sameSize = _lcpData.GetCellCountX() == tex.width && _lcpData.GetCellCountY() == tex.height;
            if(!sameSize)
            {
                Engine.Message(null, Engine.LogType.Warning, "Texture provided does not match the given LCP data size.");
                return;
            }

            _lcpDomainMeshRenderer.material.mainTexture = tex;
        }          

        public override void SetLCPViewMode(LcpViewMode lcpViewMode)
        {
            if (_lcpData == null)
            {
                return;
            }

            if (lcpViewMode == LcpViewMode.FuelModel)
            {
                _lcpDomainMeshRenderer.material.mainTexture = _fuelModelsTexture;
            }
            else if (lcpViewMode == LcpViewMode.Elevation)
            {
                _lcpDomainMeshRenderer.material.mainTexture = _elevationTexture;
            }
            else if (lcpViewMode == LcpViewMode.Slope)
            {
                _lcpDomainMeshRenderer.material.mainTexture = _slopeTexture;
            }
            else if (lcpViewMode == LcpViewMode.Aspect)
            {
                _lcpDomainMeshRenderer.material.mainTexture = _aspectTexture;
            }
            else if (lcpViewMode == LcpViewMode.TriggerBuffer)
            {
                _lcpDomainMeshRenderer.material.mainTexture = _triggerBufferTexture;
            }
        }
        private void CheckIfNeedNewFireDomainPlane(LandscapeData newLCPData)
        {
            if (_lcpData == null || DomainVisualizerUnity.NeedNewPlane(_lcpData.GetSize(), newLCPData.GetSize(), _lcpData.GetLowerLeftUTM(), newLCPData.GetLowerLeftUTM()))
            {
                _lcpDomainMeshRenderer = DomainVisualizerUnity.CreateDomainPlane(_lcpDomainPlane, _lcpDomainMeshRenderer, newLCPData.GetSize(), newLCPData.OriginOffset);
            }
        }

        public override void SetAndDisplayLCP(LandscapeData newLCPData, LcpViewMode lcpViewMode = LcpViewMode.FuelModel)
        {   
            int xDim = newLCPData.GetCellCountX();
            int yDim = newLCPData.GetCellCountY();

            if (DomainVisualizerUnity.NeedNewTexture(newLCPData.GetCellCount(), _fuelModelsTexture))
            {
                _fuelModelsTexture = new Texture2D(xDim, yDim, TextureFormat.RGBA32, false);
                _fuelModelsTexture.filterMode = FilterMode.Point;

                _elevationTexture = new Texture2D(xDim, yDim, TextureFormat.RGBA32, false);
                _elevationTexture.filterMode = FilterMode.Point;

                _slopeTexture = new Texture2D(xDim, yDim, TextureFormat.RGBA32, false);
                _slopeTexture.filterMode = FilterMode.Point;

                _aspectTexture = new Texture2D(xDim, yDim, TextureFormat.RGBA32, false);
                _aspectTexture.filterMode = FilterMode.Point;
            }

            CheckIfNeedNewFireDomainPlane(newLCPData);

            //set data
            _lcpData = newLCPData;           

            //update textures
            Vector2d elevationMinMax = _lcpData.GetElevationMinMax();
            Vector2d slopeMinMax = _lcpData.GetSlopeMinMax();
            Vector2d aspectMinMax = _lcpData.GetAspectMinMax();

            float elevationRange = (float)(elevationMinMax.y - elevationMinMax.x);
            float slopeRange = (float)(slopeMinMax.y - slopeMinMax.x);
            float aspectRange = (float)(aspectMinMax.y - aspectMinMax.x);
            float alpha = 0.85f;

            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    LandscapeCellData l = _lcpData.GetCellDataSimulationIndex(x, y, false);

                    PREACTColor c = FuelModelColors.GetFuelColor((int)l.fuel_model);
                    c.a = alpha;
                    _fuelModelsTexture.SetPixel(x, y, c.UnityColor());

                    c = PREACTColor.white * ((l.elevation - (float)elevationMinMax.x) / elevationRange);
                    c.a = alpha;
                    _elevationTexture.SetPixel(x, y, c.UnityColor());

                    c = PREACTColor.white * ((l.slope - (float)slopeMinMax.x) / slopeRange);
                    c.a = alpha;
                    _slopeTexture.SetPixel(x, y, c.UnityColor());

                    c = PREACTColor.white * ((l.aspect - (float)aspectMinMax.x) / aspectRange);
                    c.a = alpha;
                    _aspectTexture.SetPixel(x, y, c.UnityColor());
                }
            }

            _fuelModelsTexture.Apply();
            _elevationTexture.Apply();
            _slopeTexture.Apply();
            _aspectTexture.Apply();
                        
            SetLCPViewMode(LcpViewMode.FuelModel);
            SetVisibility(true);
        }

        public override void DisplayTriggerBuffer(float[,] data)
        {
            if(_lcpData == null)
            {
                Engine.Message(null, Engine.LogType.Warning, "Cannot display trigger buffer without specified Landscape data.");
                return;
            }

            int xPixels = data.GetLength(0);
            int yPixels = data.GetLength(1);
            bool sameSize = _lcpData.GetCellCountX() == data.GetLength(0) && _lcpData.GetCellCountY() == data.GetLength(1);
            if (!sameSize)
            {
                Engine.Message(null, Engine.LogType.Warning, "Trigger buffer provided does not match the given LCP data.");
                return;
            }                     

            if(DomainVisualizerUnity.NeedNewTexture(new Vector2int(xPixels, yPixels), _triggerBufferTexture))
            {
                _triggerBufferTexture = new Texture2D(xPixels, yPixels, TextureFormat.RGBA32, false);
                _triggerBufferTexture.filterMode = FilterMode.Point;
            }

            for (int y = 0; y < yPixels; y++)
            {
                for (int x = 0; x < xPixels; x++)
                {
                    float value = data[x, y];
                    //add banding
                    float band = (int)(value * 5) / 5f;
                    Color c = Color.red * (0.8f * band + 0.2f);
                    c.a = 1.0f;
                    if (value == 0f)
                    {
                        c.a = 0f;
                    }
                    _triggerBufferTexture.SetPixel(x, y, c);
                }
            }

            _triggerBufferTexture.Apply();
            SetLCPViewMode(LcpViewMode.TriggerBuffer);
            SetVisibility(true);
        }

        public override void SetVisibility(bool visible)
        {
            _lcpDomainPlane.SetActive(visible);
        }

        public override bool ToggleVisibility()
        {
            _lcpDomainPlane.SetActive(!_lcpDomainPlane.activeSelf);
            return _lcpDomainPlane.activeSelf;
        }

        public override bool IsDataPlaneActive()
        {
            return _lcpDomainPlane.activeSelf;
        }
    }
}

