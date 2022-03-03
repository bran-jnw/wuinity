using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvacuationRenderer : MonoBehaviour
{
    [SerializeField] Material householdsMaterial;
    [SerializeField] Mesh householdMesh;
    [SerializeField] Material carsMaterial;
    [SerializeField] Mesh carMesh;

    Bounds bounds;
    ComputeBuffer householdPositionsBuffer;
    bool renderHouseholds = false;
    bool renderCars = false;
    Vector4[] carPositionsArray;

    ComputeBuffer carPositionsBuffer;


    public void CreateHouseholdsBuffer(int householdCount, Vector2D domainSize)
    {
        if(householdPositionsBuffer != null)
        {
            Release();
        }

        Vector3 center = new Vector3((float)domainSize.x * 0.5f, 1f, (float)domainSize.y * 0.5f);
        Vector3 size = new Vector3((float)domainSize.x + 2f, 2f, (float)domainSize.y + 2f);
        bounds = new Bounds(center, size);
        householdPositionsBuffer = new ComputeBuffer(householdCount, 4 * sizeof(float));
        renderHouseholds = true;
    }

    void Update()
    {
        if(renderHouseholds)
        {
            Vector4[] newPositions = WUInity.WUInity.SIM.GetMacroHumanSim().GetHouseholdPositions();
            householdPositionsBuffer.SetData(newPositions);
            householdsMaterial.SetBuffer("_PositionsAndState", householdPositionsBuffer);
            Graphics.DrawMeshInstancedProcedural(householdMesh, 0, householdsMaterial, bounds, householdPositionsBuffer.count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
        }
        
        if(renderCars)
        {
            if (carPositionsBuffer != null)
            {
                carPositionsBuffer.Release();
                carPositionsBuffer = null;
            }
            carPositionsBuffer = new ComputeBuffer(carPositionsArray.Length, 4 * sizeof(float));
            carPositionsBuffer.SetData(carPositionsArray);
            carsMaterial.SetBuffer("_PositionsAndState", carPositionsBuffer);
            Graphics.DrawMeshInstancedProcedural(carMesh, 0, carsMaterial, bounds, carPositionsBuffer.count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
        }
    }

    public void UpdateCarsToRender(Vector4[] newPositions)
    {
        renderCars = true;
        carPositionsArray = newPositions;
    }

    void OnDisable()
    {
        Release();    
    }

    void Release()
    {
        householdPositionsBuffer.Release();
        householdPositionsBuffer = null;
        carPositionsBuffer.Release();
        carPositionsBuffer = null;
    }
}
