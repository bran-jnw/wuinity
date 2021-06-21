using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace WUInity.Fire
{

    public class WUInityFireTester : MonoBehaviour
    {
        [SerializeField] MeshFilter terrainMeshFilter;   
        [SerializeField] Material fireMaterial;

        [SerializeField] SpreadMode spreadMode = SpreadMode.SixteenDirections;
        [SerializeField] Vector2Int cellCount = new Vector2Int(128, 128);
        [SerializeField] Vector2D cellSize = new Vector2D(30, 30);
        [SerializeField] WindInput wind = WindInput.GetTemplate();
        [SerializeField] WeatherInput weather = WeatherInput.GetTemplate();
        [SerializeField] InitialFuelMoistureData initialMoisture = InitialFuelMoistureData.GetDefaults();
        [SerializeField] int fuelModel = 1;
        [SerializeField] float slope = 0.1f;
        
        float northElevation = 0f;
        float southElevation = 0f;


        [SerializeField] WUInityFireMesh mesh;
        LCPData lcpData;
        WUInityFireIgnition[] ignitionPoints;
        float time = 0f;
        bool simulate = false;

        void Start()
        {
            northElevation = (float)(slope * cellCount.y * cellSize.y);
            CreateLCPData();

            ignitionPoints = new WUInityFireIgnition[1];
            ignitionPoints[0] = new WUInityFireIgnition(cellCount.x / 2, cellCount.y / 2);
            /*ignitionPoints = new WUInityFireIgnition[randomIgnPoints];
            for (int i = 0; i < ignitionPoints.Length; i++)
            {
                ignitionPoints[i] = new WUInityFireIgnition(UnityEngine.Random.Range(0, xCells), UnityEngine.Random.Range(0, yCells));
            }*/

            mesh = new WUInityFireMesh(lcpData, weather, wind, initialMoisture, ignitionPoints);
            mesh.terrainMesh = terrainMeshFilter.mesh;
            mesh.spreadMode = spreadMode;
            //start simulation and do the init
            mesh.Simulate();
            fireMaterial.mainTexture = mesh.burnTexture;

        }

        void CreateLCPData()
        {
            lcpData = new LCPData();
            lcpData.RasterCellResolutionX = cellSize.x;
            lcpData.RasterCellResolutionY = cellSize.y;
            lcpData.Header.numnorth = cellCount.y;
            lcpData.Header.numeast = cellCount.x;
            lcpData.NumVals = 10;
            lcpData.Header.loelev = 0;

            //always save ten values per cell, so all of them
            int NumVals = 10;            
            short[] landscape = new short[cellCount.x * cellCount.y * NumVals];

            for (int i = 0; i < cellCount.y; i++)
            {
                for (int j = 0; j < cellCount.x; j++)
                {
                    //elevation
                    landscape[i * cellCount.x * NumVals + j * NumVals + 0] = (short)GetElevation(i);
                    // slope
                    landscape[i * cellCount.x * NumVals + j * NumVals + 1] = -1;
                    // aspect
                    landscape[i * cellCount.x * NumVals + j * NumVals + 2] = -1;
                    // fuel model
                    landscape[i * cellCount.x * NumVals + j * NumVals + 3] = (short)fuelModel;
                    // canopy cover
                    landscape[i * cellCount.x * NumVals + j * NumVals + 4] = 0;
                    // canopy height
                    landscape[i * cellCount.x * NumVals + j * NumVals + 5] = 0;
                    // crown base
                    landscape[i * cellCount.x * NumVals + j * NumVals + 6] = 0;
                    // bulk density
                    landscape[i * cellCount.x * NumVals + j * NumVals + 7] = 0;
                    // duff model
                    landscape[i * cellCount.x * NumVals + j * NumVals + 8] = 0;
                    // coarse woody model
                    landscape[i * cellCount.x * NumVals + j * NumVals + 9] = 0;          
                }
            }

            lcpData.landscape = landscape;
        }

        float GetElevation(int yPos)
        {
            //reverse order on y axis due to how LCP format is made
            return Mathf.Lerp(southElevation, northElevation, (cellCount.y - 1 - yPos) / (float)(cellCount.y - 1));
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                simulate = !simulate;
            }

            if (simulate)
            {
                simulate = mesh.Simulate();
                if (simulate)
                {
                    time += (float)mesh.dt;
                }

            }
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 500, 20), "Elapsed time: " + time / 3600.0f + " hours");
            if (mesh != null)
            {
                GUI.Label(new Rect(10, 30, 500, 20), "Last time step: " + mesh.dt + " seconds");
                if (mesh.activeCells != null)
                {
                    GUI.Label(new Rect(10, 50, 500, 20), "Active cells: " + mesh.activeCells.Count);
                }
            }
        }
    }
}
