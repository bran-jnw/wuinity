//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using PREACT.Pedestrian;
using System.Collections.Generic;
using PREACT;
using PREACT.Traffic;
using PREACT.Utility.Math;

namespace WUInity.Visualization
{
    public class EvacuationRenderer : MonoBehaviour
    {
        [SerializeField] Material householdsMaterial;
        [SerializeField] Mesh householdMesh;
        [SerializeField] Material carsMaterial;
        [SerializeField] Mesh carMesh;

        Bounds bounds;
        ComputeBuffer householdPositionsBuffer;        
        ComputeBuffer carPositionsBuffer;
        Dictionary<uint, TrafficModuleVehicle> _activeVehicles;


        public void CreateBuffers(bool renderHouseholds, bool renderTraffic, Vector2d domainSize, PedestrianModule pedestrianModule)
        {
            Release();

            //calculate bounds here as traffic will need it too, not only pedestrian visualizer
            Vector3 center = new Vector3((float)domainSize.x * 0.5f, 1f, (float)domainSize.y * 0.5f);
            Vector3 size = new Vector3((float)domainSize.x + 2f, 2f, (float)domainSize.y + 2f);
            bounds = new Bounds(center, size);

            if (renderHouseholds)
            {
                CreateHouseholdsBuffer(((MacroHouseholdSim)pedestrianModule).GetHouseholdPositions().Length);
            }            
        }

        private void CreateHouseholdsBuffer(int householdCount)
        {            
            householdPositionsBuffer = new ComputeBuffer(householdCount, 4 * sizeof(float));
        }

        public void UpdateEvacuationRenderer(bool renderHouseholds, bool renderCars, PedestrianModule pedestrianModule, TrafficModule trafficModule)
        {
            if (renderHouseholds)
            {
                System.Numerics.Vector4[] newPositions = ((MacroHouseholdSim)pedestrianModule).GetHouseholdPositions();
                householdPositionsBuffer.SetData(newPositions);
                householdsMaterial.SetBuffer("_PositionsAndState", householdPositionsBuffer);
                Graphics.DrawMeshInstancedProcedural(householdMesh, 0, householdsMaterial, bounds, householdPositionsBuffer.count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
            }

            if (renderCars)
            {
                Dictionary<uint, TrafficModuleVehicle> currentVehicles = trafficModule.GetActiveVehicles();
                if(currentVehicles.Count > 0)
                {
                    //need to make a copy as it might get modified during foreach
                    _activeVehicles = new Dictionary<uint, TrafficModuleVehicle>(currentVehicles);

                    if (carPositionsBuffer == null || _activeVehicles.Count != carPositionsBuffer.count)
                    {     
                        if (carPositionsBuffer != null)
                        {
                            carPositionsBuffer.Release();
                        }
                        carPositionsBuffer = new ComputeBuffer(_activeVehicles.Count, 4 * sizeof(float));
                    }
                    
                    List<Vector4> dataToRender = new List<Vector4>();
                    foreach(TrafficModuleVehicle vehicle in _activeVehicles.Values)
                    {
                        Vector2d pos = vehicle.WorldPosition;
                        float speedRatio = vehicle.SpeedRatio;
                        Vector4 data = new Vector4((float)pos.x, (float)pos.y, speedRatio, 0f);
                        dataToRender.Add(data);
                    }
                    carPositionsBuffer.SetData(dataToRender);
                    carsMaterial.SetBuffer("_PositionsAndState", carPositionsBuffer);
                    Graphics.DrawMeshInstancedProcedural(carMesh, 0, carsMaterial, bounds, carPositionsBuffer.count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
                }           
            }
        }

        void OnDisable()
        {
            Release();
        }

        void OnDestroy()
        {
            Release();
        }

        void Release()
        {
            if(householdPositionsBuffer != null)
            {
                householdPositionsBuffer.Release();
                householdPositionsBuffer = null;
            }

            if(carPositionsBuffer != null)
            {
                carPositionsBuffer.Release();
                carPositionsBuffer = null;
            }            
        }
    }
}

