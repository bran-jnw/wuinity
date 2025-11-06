//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT.Population;
using PREACT;

namespace WUInity.Population
{
    public class PopulationVisualizerUnity : PopulationVisualizer
    {
        WUInityManager _manager;
        private GameObject _LocalGPWDataPlane;
        private Material _gpwDataPlaneMaterial;

        private Texture2D _populationMapTexture;
        private Texture2D _populationMapMaskTexture;
        private Texture2D _localGPWTexture;

        private LocalGPWData _localGPWData;
        private PopulationMap _populationMap;

        public LocalGPWData LocalGPWData { get => _localGPWData; }        
        public PopulationMap PopulationMap { get => _populationMap; }
        public GameObject DataPlane
        {
            get
            {
                if (_LocalGPWDataPlane == null)
                {
                    if (_localGPWData != null)
                    {
                        CreateLocalGPWDataPlane(_localGPWData);
                    }
                    else
                    {
                        Engine.MESSAGE(null, Engine.LogType.Warning, "Local GPW data not loaded.");
                    }
                }

                return _LocalGPWDataPlane;
            }
        }
        
        public PopulationVisualizerUnity(WUInityManager manager)
        { 
            _manager = manager;
        }

        public override bool IsDataPlaneActive()
        {
            return DataPlane.activeSelf;
        }

        public override object GetPopulationTexture()
        {
            return _populationMapTexture;
        }

        public override object GetPopulationMaskTexture()
        {
            return _populationMapMaskTexture;
        }

        public override void SetDataPlane(bool activeSelf)
        {
            DataPlane.SetActive(activeSelf);
        }

        public override bool ToggleLocalGPWVisibility()
        {
            DataPlane.SetActive(!_LocalGPWDataPlane.activeSelf);

            return DataPlane.activeSelf;
        }

        public override void DisplayLocalGPW(LocalGPWData data)
        {
            if (_localGPWData == null || data != _localGPWData)
            {
                _localGPWTexture = new Texture2D(data.CellCount.x, data.CellCount.y);
                _localGPWTexture.filterMode = FilterMode.Point;
            }
            _localGPWData = data;
            
            for (int y = 0; y < data.CellCount.y; y++)
            {
                for (int x = 0; x < data.CellCount.x; x++)
                {
                    double density = data.GetDensity(x, y);
                    PREACTColor color = GetGPWColor((float)density);

                    _localGPWTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            _localGPWTexture.Apply();


        }

        public override void DisplayPopulationMap(PopulationMap data)
        {
            if(_populationMapTexture == null || data != _populationMap)
            {
                _populationMapTexture = new Texture2D(data._cells.x, data._cells.y);
                _populationMapTexture.filterMode = FilterMode.Point;
            }
            _populationMap = data;

            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.x; x++)
                {
                    double density = data.GetPeopleCount(x, y) / data._cellArea;
                    PREACTColor color = GetGPWColor((float)density);
                    if(density == 0)
                    {
                        color.a = 0f;
                    }

                    _populationMapTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            _populationMapTexture.Apply();
        }

        public override void CreatePopulationMapMaskTexture(PopulationMap data)
        {
            if (_populationMapMaskTexture == null || !(_populationMapMaskTexture.width == data._cells.x && _populationMapMaskTexture.height == data._cells.y))
            {
                _populationMapMaskTexture = new Texture2D(data._cells.x, data._cells.y);
                _populationMapMaskTexture.filterMode = FilterMode.Point;
            }

            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.x; x++)
                {
                    if(data.GetMaskValue(x, y))
                    {
                        Color color = Color.red;
                        color.a = 0.5f;
                        _populationMapMaskTexture.SetPixel(x, y, color);
                    }                    
                }
            }
            _populationMapMaskTexture.Apply();
        }

        private void CreateLocalGPWDataPlane(LocalGPWData localGPWData)
        {
            Mesh mesh;
            MeshRenderer mR;
            MeshFilter filter;

            if (_LocalGPWDataPlane == null)
            {
                _LocalGPWDataPlane = new GameObject("GPWDensityMap");
                _LocalGPWDataPlane.transform.parent = _manager.transform;
                _LocalGPWDataPlane.isStatic = true;
                filter = _LocalGPWDataPlane.AddComponent<MeshFilter>();
                mesh = new Mesh(); // filter.mesh;
                filter.mesh = mesh;
                mR = _LocalGPWDataPlane.AddComponent<MeshRenderer>();
                mR.receiveShadows = false;
                mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                mR = _LocalGPWDataPlane.GetComponent<MeshRenderer>();
                mesh = _LocalGPWDataPlane.GetComponent<MeshFilter>().mesh;
                filter = _LocalGPWDataPlane.GetComponent<MeshFilter>();
            }

            mesh.Clear();

            float width = (float)localGPWData.RealWorldSize.x; //(float)size.x;
            float length = (float)localGPWData.RealWorldSize.y; //(float)size.y;

            Vector3 offset = new Vector3((float)localGPWData.OriginOffset.x, 0.0f, (float)localGPWData.OriginOffset.y);

            WUInity.Visualization.VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset);

            if (_gpwDataPlaneMaterial == null)
            {
                _gpwDataPlaneMaterial = new Material(Shader.Find("Unlit/Transparent"));
            }
            _gpwDataPlaneMaterial.mainTexture = _localGPWTexture;

            mR.material = _gpwDataPlaneMaterial;
            //filter.mesh = mesh;

            //move up one meter
            _LocalGPWDataPlane.transform.position = Vector3.up;
            _LocalGPWDataPlane.SetActive(false);
        }        
    }
}