using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseholdRenderer : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Mesh mesh;
    Bounds bounds;
    ComputeBuffer positionsBuffer;
    bool render = false;


    public void CreateBuffer(int householdCount, Vector2D domainSize)
    {
        if(positionsBuffer != null)
        {
            Release();
        }

        Vector3 center = new Vector3((float)domainSize.x * 0.5f, 1f, (float)domainSize.y * 0.5f);
        Vector3 size = new Vector3((float)domainSize.x + 2f, 2f, (float)domainSize.y + 2f);
        bounds = new Bounds(center, size);
        positionsBuffer = new ComputeBuffer(householdCount, 4 * sizeof(float));
        render = true;
    }

    void Update()
    {
        if(render)
        {
            Vector4[] newPositions = WUInity.WUInity.SIM.GetMacroHumanSim().GetHouseholdPositions();
            positionsBuffer.SetData(newPositions);
            material.SetBuffer("_PositionsAndState", positionsBuffer);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count, null, UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off, null);
        }        
    }

    void OnDisable()
    {
        Release();    
    }

    void Release()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }
}
