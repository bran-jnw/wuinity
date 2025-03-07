﻿//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using WUIPlatform.Fire;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity
{
    public class WUInityFireTester : MonoBehaviour                                  //create class with standard unity class inheritance
    {
        [SerializeField] MeshFilter terrainMeshFilter;   
        [SerializeField] Material fireMaterial;

        [SerializeField] FireCellInput.SpreadModeEnum spreadMode = FireCellInput.SpreadModeEnum.SixteenDirections;      //Sixteen direction spread mode
        [SerializeField] Vector2int cellCount = new Vector2int(128, 128);           //128 x 128 raster
        [SerializeField] Vector2d cellSize = new Vector2d(30, 30);                  //30 x 30 m cells
        [SerializeField] WindInput wind = WindInput.GetTemplate();                  //Wind data from template (default in code)
        [SerializeField] WeatherInput weather = WeatherInput.GetTemplate();         //same as above
        [SerializeField] InitialFuelMoistureLibrary initialMoisture = new InitialFuelMoistureLibrary();       //same as above
        [SerializeField] int fuelModel = 1;                                         //all cells are fuel model 1 (grass I think)
        [SerializeField] float slope = 0.1f;                                        //10% slope
        
        float northElevation = 0f;                                                  //elevation??
        float southElevation = 0f;


        [SerializeField] FireMesh mesh;          //declare other variables
        LCPData lcpData;
        IgnitionPoint[] ignitionPoints;
        float time = 0f;
        bool simulate = false;

        void Start()                
        {
            northElevation = (float)(slope * cellCount.y * cellSize.y);                 
            CreateLCPData();                    

            ignitionPoints = new IgnitionPoint[1];                                    
            ignitionPoints[0] = new IgnitionPoint(cellCount.x / 2, cellCount.y / 2, 0.0f); 
            /*ignitionPoints = new WUInityFireIgnition[randomIgnPoints];
            for (int i = 0; i < ignitionPoints.Length; i++)
            {
                ignitionPoints[i] = new WUInityFireIgnition(UnityEngine.Random.Range(0, xCells), UnityEngine.Random.Range(0, yCells));
            }*/

            mesh = new FireMesh(lcpData, weather, wind, initialMoisture, ignitionPoints);                                                  
            mesh.spreadMode = spreadMode;                                                              
            //start simulation and do the init
            mesh.Step(time, 1f);                                           

        }

        void CreateLCPData()                                                                    
        {
            lcpData = new LCPData(cellSize, cellCount);                                                             

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
                mesh.Step(time, 1f);
                simulate = mesh.IsSimulationDone();
                if (simulate)
                {
                    time += (float)mesh.dt;
                }

            }
        }

        void OnGUI()                                                                           
        {
            UnityEngine.GUI.Label(new Rect(10, 10, 500, 20), "Elapsed time: " + time / 3600.0f + " hours");
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
