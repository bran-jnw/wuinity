//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using WUIPlatform.Pedestrian;

namespace WUIPlatform.WUInity.Visualization
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
        System.Numerics.Vector4[] carPositionsArray;


        public void CreateBuffers(bool renderHouseholds, bool renderTraffic)
        {
            Release();

            if (renderHouseholds)
            {
                CreateHouseholdsBuffer(((MacroHouseholdSim)WUIEngine.SIM.PedestrianModule).GetHouseholdPositions().Length, WUIEngine.INPUT.Simulation.Size);
            }            
        }

        private void CreateHouseholdsBuffer(int householdCount, Vector2d domainSize)
        {
            Vector3 center = new Vector3((float)domainSize.x * 0.5f, 1f, (float)domainSize.y * 0.5f);
            Vector3 size = new Vector3((float)domainSize.x + 2f, 2f, (float)domainSize.y + 2f);
            bounds = new Bounds(center, size);
            householdPositionsBuffer = new ComputeBuffer(householdCount, 4 * sizeof(float));
        }

        public void UpdateEvacuationRenderer(bool renderHouseholds, bool renderCars)
        {
            if (renderHouseholds)
            {
                System.Numerics.Vector4[] newPositions = ((MacroHouseholdSim)WUIEngine.SIM.PedestrianModule).GetHouseholdPositions();
                householdPositionsBuffer.SetData(newPositions);
                householdsMaterial.SetBuffer("_PositionsAndState", householdPositionsBuffer);
                Graphics.DrawMeshInstancedProcedural(householdMesh, 0, householdsMaterial, bounds, householdPositionsBuffer.count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
            }

            if (renderCars)
            {
                if (carPositionsBuffer != null)
                {
                    carPositionsBuffer.Release();
                    carPositionsBuffer = null;
                }
                if(WUIEngine.SIM.TrafficModule.GetCarsInSystem() > 0)
                {
                    carPositionsArray = WUIEngine.SIM.TrafficModule.GetCarPositionsAndStates();
                    if(carPositionsBuffer == null || carPositionsArray.Length != carPositionsBuffer.count)
                    {
                        carPositionsBuffer = new ComputeBuffer(carPositionsArray.Length, 4 * sizeof(float));
                    }                    
                    carPositionsBuffer.SetData(carPositionsArray);
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

