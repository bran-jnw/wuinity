using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Vector4[] carPositionsArray;


        public void CreateBuffers(bool renderHouseholds, bool renderTraffic)
        {
            Release();

            if (renderHouseholds)
            {
                CreateHouseholdsBuffer(WUInity.SIM.GetMacroHumanSim().GetHouseholdPositions().Length, WUInity.INPUT.size);
            }            
        }

        private void CreateHouseholdsBuffer(int householdCount, Vector2D domainSize)
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
                Vector4[] newPositions = WUInity.SIM.GetMacroHumanSim().GetHouseholdPositions();
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
                if(WUInity.SIM.GetMacroTrafficSim().GetCarsInSystem() > 0)
                {
                    carPositionsArray = WUInity.SIM.GetMacroTrafficSim().GetCarPositionsAndStates();
                    carPositionsBuffer = new ComputeBuffer(carPositionsArray.Length, 4 * sizeof(float));
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

