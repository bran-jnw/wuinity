using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace WUInity.Fire
{
    public class WUInityFireTester : MonoBehaviour                                  //create class with standard unity class inheritance
    {
        [SerializeField] MeshFilter terrainMeshFilter;   
        [SerializeField] Material fireMaterial;

        [SerializeField] SpreadMode spreadMode = SpreadMode.SixteenDirections;      //Sixteen direction spread mode
        [SerializeField] Vector2Int cellCount = new Vector2Int(128, 128);           //128 x 128 raster
        [SerializeField] Vector2D cellSize = new Vector2D(30, 30);                  //30 x 30 m cells
        [SerializeField] WindInput wind = WindInput.GetTemplate();                  //Wind data from template (default in code)
        [SerializeField] WeatherInput weather = WeatherInput.GetTemplate();         //same as above
        [SerializeField] InitialFuelMoistureData initialMoisture = InitialFuelMoistureData.GetDefaults();       //same as above
        [SerializeField] int fuelModel = 1;                                         //all cells are fuel model 1 (grass I think)
        [SerializeField] float slope = 0.1f;                                        //10% slope
        
        float northElevation = 0f;                                                  //elevation??
        float southElevation = 0f;


        [SerializeField] FireMesh mesh;          //declare other variables
        LCPData lcpData;
        IgnitionPoint[] ignitionPoints;
        float time = 0f;
        bool simulate = false;

        void Start()                //standard unity method that runs every time on startup.
        {
            northElevation = (float)(slope * cellCount.y * cellSize.y);                 //set maximum elevation??
            CreateLCPData();                    

            ignitionPoints = new IgnitionPoint[1];                                    //create one ignition point
            ignitionPoints[0] = new IgnitionPoint(cellCount.x / 2, cellCount.y / 2);  //at the center of the domain
            /*ignitionPoints = new WUInityFireIgnition[randomIgnPoints];
            for (int i = 0; i < ignitionPoints.Length; i++)
            {
                ignitionPoints[i] = new WUInityFireIgnition(UnityEngine.Random.Range(0, xCells), UnityEngine.Random.Range(0, yCells));
            }*/

            mesh = new FireMesh(lcpData, weather, wind, initialMoisture, ignitionPoints);        //create the fire mesh from constructor
            mesh.terrainMesh = terrainMeshFilter.mesh;                                                  //guessing this is for rendering?
            mesh.spreadMode = spreadMode;                                                               //set spread mode
            //start simulation and do the init
            mesh.Simulate();                                                                            //run simulation one, do initiation
            fireMaterial.mainTexture = mesh.burnTexture;                                                //also for rendering

        }

        void CreateLCPData()                                                                    //method to create artificial landscape
        {
            lcpData = new LCPData();                                                            //create lcpData
            lcpData.RasterCellResolutionX = cellSize.x;                                         //sets its straightforward values
            lcpData.RasterCellResolutionY = cellSize.y;
            lcpData.Header.numnorth = cellCount.y;
            lcpData.Header.numeast = cellCount.x;
            lcpData.NumVals = 10;                                                               //dont really know, why are we saving ten values of each?
            lcpData.Header.loelev = 0;                                                          //lowest elevation set

            //always save ten values per cell, so all of them
            int NumVals = 10;            
            short[] landscape = new short[cellCount.x * cellCount.y * NumVals];

            for (int i = 0; i < cellCount.y; i++)                                               //for all the cells, set the landscape var (what does each cell contain?)
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

            lcpData.landscape = landscape;                                                      //parse landscape to lcpdata
        }

        float GetElevation(int yPos)
        {
            //reverse order on y axis due to how LCP format is made
            return Mathf.Lerp(southElevation, northElevation, (cellCount.y - 1 - yPos) / (float)(cellCount.y - 1));
        }

        void Update()                                                                           //standard unity method that runs every frame(ish)
        {
            if (Input.GetKeyDown(KeyCode.Space))                                                //so spacebar is pause?
            {
                simulate = !simulate;
            }

            if (simulate)
            {
                simulate = mesh.Simulate();                                                     //run the simulation for every time step.
                if (simulate)
                {
                    time += (float)mesh.dt;
                }

            }
        }

        void OnGUI()                                                                            //method to pass things to the GUI
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
