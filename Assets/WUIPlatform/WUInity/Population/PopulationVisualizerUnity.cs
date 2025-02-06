//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using WUIPlatform.Runtime;

namespace WUIPlatform.Population
{
    public class PopulationVisualizerUnity : PopulationVisualizer
    {
        private GameObject _LocalGPWDataPlane;
        private Material gpwDataPlaneMaterial;

        private Texture2D populationMapTexture;
        private Texture2D populationMapMaskTexture;
        private Texture2D densityTexture;

        public PopulationVisualizerUnity(PopulationData owner) : base(owner)
        { 

        }

        public override bool IsDataPlaneActive()
        {
            return DataPlane.activeSelf;
        }

        public override object GetPopulationTexture()
        {
            return populationMapTexture;
        }

        public override object GetPopulationMaskTexture()
        {
            return populationMapMaskTexture;
        }

        public GameObject DataPlane
        {
            get
            {
                if (_LocalGPWDataPlane == null)
                {
                    if(WUIEngine.RUNTIME_DATA.Population.LocalGPWData != null)
                    {
                        CreateLocalGPWDataPlane(WUIEngine.RUNTIME_DATA.Population.LocalGPWData);
                    }
                    else
                    {
                        WUIEngine.LOG(WUIEngine.LogType.Warning, "Local GPW data not loaded.");
                    }
                }

                return _LocalGPWDataPlane;
            }
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

        public override void CreatePopulationMapTexture(PopulationMap data)
        {
            if(populationMapTexture == null || !(populationMapTexture.width == data._cells.x && populationMapTexture.height == data._cells.y))
            {
                populationMapTexture = new Texture2D(data._cells.x, data._cells.y);
                populationMapTexture.filterMode = FilterMode.Point;
            }            
            
            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.x; x++)
                {
                    double density = data.GetPeopleCount(x, y) / data._cellArea;
                    WUIEngineColor color = GetGPWColor((float)density);
                    if(density == 0)
                    {
                        color.a = 0f;
                    }

                    populationMapTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            populationMapTexture.Apply();
        }

        public override void CreatePopulationMapMaskTexture(PopulationMap data)
        {
            if (populationMapMaskTexture == null || !(populationMapMaskTexture.width == data._cells.x && populationMapMaskTexture.height == data._cells.y))
            {
                populationMapMaskTexture = new Texture2D(data._cells.x, data._cells.y);
                populationMapMaskTexture.filterMode = FilterMode.Point;
            }

            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.y; x++)
                {
                    if(data.GetMaskValue(x, y))
                    {
                        Color color = Color.red;
                        color.a = 0.5f;
                        populationMapMaskTexture.SetPixel(x, y, color);
                    }                    
                }
            }
            populationMapMaskTexture.Apply();
        }

        private void CreateLocalGPWDataPlane(LocalGPWData localGPWData)
        {
            Mesh mesh;
            MeshRenderer mR;
            MeshFilter filter;

            if (_LocalGPWDataPlane == null)
            {
                _LocalGPWDataPlane = new GameObject("GPWDensityMap");
                _LocalGPWDataPlane.transform.parent = WUInity.WUInityEngine.INSTANCE.transform;
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

            float width = (float)localGPWData.realWorldSize.x; //(float)size.x;
            float length = (float)localGPWData.realWorldSize.y; //(float)size.y;

            Vector3 offset = new Vector3((float)localGPWData.originOffset.x, 0.0f, (float)localGPWData.originOffset.y);

            WUInity.Visualization.VisualizeUtilities.CreateSimplePlane(mesh, width, length, 0.0f, offset);

            if (gpwDataPlaneMaterial == null)
            {
                gpwDataPlaneMaterial = new Material(Shader.Find("Unlit/Transparent"));
            }
            gpwDataPlaneMaterial.mainTexture = densityTexture;

            mR.material = gpwDataPlaneMaterial;
            //filter.mesh = mesh;

            //move up one meter
            _LocalGPWDataPlane.transform.position = Vector3.up;
            _LocalGPWDataPlane.SetActive(false);
        }

        public override void CreateGPWTexture(LocalGPWData data)
        {
            densityTexture = new Texture2D(data._cells.x, data._cells.y);
            densityTexture.filterMode = UnityEngine.FilterMode.Point;
            for (int y = 0; y < data._cells.y; y++)
            {
                for (int x = 0; x < data._cells.x; x++)
                {
                    double density = data.GetDensity(x, y);
                    WUIEngineColor color = GetGPWColor((float)density);

                    densityTexture.SetPixel(x, y, color.UnityColor);
                }
            }
            densityTexture.Apply();
        }
    }
}