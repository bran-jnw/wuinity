//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace WUIPlatform.Traffic
{
    public class CityFlowModule : TrafficModule
    {
        private IntPtr engine;

        [DllImport("cityflow_unity.dll")]
        private static extern int Test();
        [DllImport("cityflow_unity.dll")]
        private static extern IntPtr CreateEngine(string configFile, int threadNum);
        [DllImport("cityflow_unity.dll")]
        private static extern int NextStep(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern int GetVehicleCount(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern double GetCurrentTime(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern int GetSuccess(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern string GetFilePath(IntPtr engine);
        [DllImport("cityflow_unity.dll")]
        private static extern IntPtr GetVehicle(IntPtr engine, int index);
        [DllImport("cityflow_unity.dll")]
        private static extern void GetVehicles(IntPtr engine);

        public CityFlowModule()
        {
            //MonoBehaviour.print(Test());
            engine = CreateEngine("D:\\UNITY\\_PROJECTS\\CityFlow\\examples\\config.json", 1);
            for (int i = 0; i < 600; i++)
            {
                
            }
            //MonoBehaviour.print(GetCurrentTime(engine));
            //MonoBehaviour.print(GetSuccess(engine));
            //print(GetFilePath(engine));
            //print(GetFilePath(engine));
            //print(GetVehicle(engine));
        }

        public override void Step(float deltaTime, float currentTime)
        {
            NextStep(engine);
            int vehicles = GetVehicleCount(engine);
            //GetVehicles(engine);
            for (int j = 0; j < vehicles; ++j)
            {
                IntPtr b = GetVehicle(engine, j);
                string c = Marshal.PtrToStringAnsi(b);
                //MonoBehaviour.print(c);
                //print(vehicles);
            }
        }

        public override bool IsSimulationDone()
        {
            throw new NotImplementedException();
        }

        public override void InsertNewTrafficEvent(TrafficEvent tE)
        {
            throw new NotImplementedException();
        }

        public override int GetTotalCarsSimulated()
        {
            throw new NotImplementedException();
        }

        public override int GetCarsInSystem()
        {
            throw new NotImplementedException();
        }

        public override void UpdateEvacuationGoals()
        {
            throw new NotImplementedException();
        }

        public override System.Numerics.Vector4[] GetCarPositionsAndStates()
        {
            throw new NotImplementedException();
        }

        public override void SaveToFile(int runNumber)
        {
            throw new NotImplementedException();
        }

        public override void HandleNewCars()
        {
            throw new NotImplementedException();
        }

        public override void HandleIgnitedFireCells(List<Vector2int> cellIndices)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}

