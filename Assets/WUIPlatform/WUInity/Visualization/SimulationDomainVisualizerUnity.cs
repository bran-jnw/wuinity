//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT.Population;
using PREACT;
using PREACT.Runtime;
using PREACT.Utility.Math;
using PREACT.IO;

namespace WUInity.Visualization
{
    public class SimulationDomainVisualizerUnity : SimulationDomainVisualizer
    {
        private GameObject _simulationDomainPlane;
        MeshRenderer _simulationDomainMeshRenderer;
        //textures
        private Texture2D _populationMapTexture;
        private Texture2D _populationMapMaskTexture;

        //TODO: worth moving to its own visualizer?
        private GameObject _gpwDomainPlane;
        MeshRenderer _gpwDomainMeshRenderer;
        //textures
        private Texture2D _localGPWTexture;

        private Vector2d _simulationDomainSize, _simulationDomainLatLon;
        private Vector2d _gpwDomanSize, _gpwDomainLatLon;

        //markers
        GameObject[] _goalMarkers;

        public SimulationDomainVisualizerUnity(Transform parent)
        {
            _simulationDomainPlane = new GameObject("SimulationDomain");            
            _simulationDomainPlane.transform.parent = parent;
            _simulationDomainPlane.transform.position += Vector3.up;
            _simulationDomainPlane.isStatic = true;

            _gpwDomainPlane = new GameObject("GPWDomain");            
            _gpwDomainPlane.transform.parent = parent;
            _gpwDomainPlane.transform.position += Vector3.up;
            _gpwDomainPlane.isStatic = true;            
        }

        public void SetSimulationPlaneTexture(Texture2D tex)
        {
            if (_simulationDomainMeshRenderer == null)
            {
                Engine.Message(null, Engine.LogType.Warning, "Cannot show simulation domain data as no domain has been set.");
                return;
            }
            _simulationDomainMeshRenderer.material.mainTexture = tex;
        }

        private void CheckIfNeedNewSimulationDomainPlane(PopulationMap data)
        {
            if (DomainVisualizerUnity.NeedNewPlane(_simulationDomainSize, data._size, _simulationDomainLatLon, data._lowerLeftLatLong))
            {
                _simulationDomainMeshRenderer = DomainVisualizerUnity.CreateDomainPlane(_simulationDomainPlane, _simulationDomainMeshRenderer, data._size, Vector2d.zero);
            }
            _simulationDomainSize = data._size;
            _simulationDomainLatLon = data._lowerLeftLatLong;
        }
        private void CheckIfNeedNewGPWDomainPlane(LocalGPWData data)
        {
            if (DomainVisualizerUnity.NeedNewPlane(_gpwDomanSize, data.RealWorldSize, _gpwDomainLatLon, data.ActualOriginLatLon))
            {
                _gpwDomainMeshRenderer = DomainVisualizerUnity.CreateDomainPlane(_gpwDomainPlane, _gpwDomainMeshRenderer, data.RealWorldSize, data.OriginOffset);
            }
            _gpwDomanSize = data.RealWorldSize;
            _gpwDomainLatLon = data.ActualOriginLatLon;
        }

        public override void SetAndDisplayPopulationMapTexture(PopulationMap data, WorkingData workingData)
        {
            if (DomainVisualizerUnity.NeedNewTexture(data._cells, _populationMapTexture))
            {
                _populationMapTexture = new Texture2D(data._cells.x, data._cells.y);
                _populationMapTexture.filterMode = FilterMode.Point;
            }

            //update cached data
            workingData.PopulationMap = data;

            CheckIfNeedNewSimulationDomainPlane(data);

            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.x; x++)
                {
                    double density = data.GetPeopleCount(x, y) / data._cellArea;
                    PREACTColor color = GetGPWColor((float)density);
                    if (density == 0)
                    {
                        color.a = 0f;
                    }

                    _populationMapTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            _populationMapTexture.Apply();
            _simulationDomainMeshRenderer.material.mainTexture = _populationMapTexture;
            SetVisibility(true);
        }

        public override void SetAndDisplayPopulationMapMaskTexture(PopulationMap data, WorkingData workingData)
        {      
            if (DomainVisualizerUnity.NeedNewTexture(data._cells, _populationMapMaskTexture))
            {
                _populationMapMaskTexture = new Texture2D(data._cells.x, data._cells.y);
                _populationMapMaskTexture.filterMode = FilterMode.Point;                
            }

            //update cached data
            workingData.PopulationMap = data;

            CheckIfNeedNewSimulationDomainPlane(data);            

            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.x; x++)
                {
                    if (data.GetMaskValue(x, y))
                    {
                        Color color = Color.red;
                        color.a = 0.5f;
                        _populationMapMaskTexture.SetPixel(x, y, color);
                    }
                }
            }
            _populationMapMaskTexture.Apply();
            _simulationDomainMeshRenderer.material.mainTexture = _populationMapMaskTexture;
            SetVisibility(true);
        }

        public override bool IsDataPlaneActive()
        {
            return _simulationDomainPlane.activeSelf;
        }

        public override object GetPopulationTexture()
        {
            return _populationMapTexture;
        }

        public override object GetPopulationMaskTexture()
        {
            return _populationMapMaskTexture;
        }

        public override void SetVisibility(bool activeSelf)
        {
            _gpwDomainPlane.SetActive(activeSelf);
        }

        public override bool ToggleVisibility()
        {
            _gpwDomainPlane.SetActive(!_gpwDomainPlane.activeSelf);

            return _gpwDomainPlane.activeSelf;
        } 


        //GPW below here
        public override void SetAndDisplayLocalGPW(LocalGPWData data, WorkingData workingData)
        {
            if (DomainVisualizerUnity.NeedNewTexture(data.CellCount, _localGPWTexture))
            {
                _localGPWTexture = new Texture2D(data.CellCount.x, data.CellCount.y);
                _localGPWTexture.filterMode = FilterMode.Point;
            }
            
            //set data
            workingData.LocalGPWData = data;

            CheckIfNeedNewGPWDomainPlane(data);

            //update texture
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
            _gpwDomainMeshRenderer.material.mainTexture = _localGPWTexture;
            SetGPWVisibility(true);
        }

        public override void SetGPWVisibility(bool visible)
        {
            _gpwDomainPlane.SetActive(visible);
        }
        public override bool ToggleGPWVisibility()
        {
            _gpwDomainPlane.SetActive(!_gpwDomainPlane.activeSelf);

            return _gpwDomainPlane.activeSelf;
        }

        public override bool IsGPWPlaneVisible()
        {
            return _gpwDomainPlane.activeSelf;
        }

        public override object GetGPWTexture()
        {
            return _localGPWTexture;
        }

        public void SpawnEvacuationGoalMarkers(PREACT.IO.PREACTInput input, GameObject markerPrefab)
        {
            if (_goalMarkers != null)
            {
                for (int i = 0; i < _goalMarkers.Length; i++)
                {
                    if (_goalMarkers[i] != null)
                    {
                        MonoBehaviour.Destroy(_goalMarkers[i]);
                    }
                }
            }

            _goalMarkers = new GameObject[input.Evacuation.Data.EvacuationDestinationInputs.Count];
            for (int i = 0; i < input.Evacuation.Data.EvacuationDestinationInputs.Count; i++)
            {
                EvacuationDestinationInput eG = input.Evacuation.Data.EvacuationDestinationInputs[i];
                _goalMarkers[i] = MonoBehaviour.Instantiate<GameObject>(markerPrefab);
                PREACT.Utility.LatLngUTMConverter.UTMResult utmPos = PREACT.Utility.LatLngUTMConverter.WGS84.convertLatLngToUtm(eG.LatLon.x, eG.LatLon.y);
                Vector2d pos = new Vector2d(utmPos.Easting, utmPos.Northing) - input.Simulation.Data.UTMOrigin;

                float scale = 0.02f * (float)input.Simulation.DomainSize.y;
                _goalMarkers[i].transform.localScale = new Vector3(scale, 100f, scale);
                _goalMarkers[i].transform.position = new Vector3((float)pos.x, 0f, (float)pos.y);
                MeshRenderer mR = _goalMarkers[i].GetComponentInChildren<MeshRenderer>();
                mR.material.color = eG.Color.UnityColor;
            }
        }
    }
}