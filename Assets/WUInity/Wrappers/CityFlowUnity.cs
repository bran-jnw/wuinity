using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class CityFlowUnity
{
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

    public CityFlowUnity()
    {
        MonoBehaviour.print(Test());
        IntPtr engine = CreateEngine("D:\\UNITY\\_PROJECTS\\CityFlow\\examples\\config.json", 1);
        for (int i = 0; i < 600; i++)
        {
            NextStep(engine);
            int vehicles = GetVehicleCount(engine);
            //GetVehicles(engine);
            for (int j = 0; j < vehicles; ++j)
            {
                IntPtr b = GetVehicle(engine, j);
                string c = Marshal.PtrToStringAnsi(b);
                MonoBehaviour.print(c);
                //print(vehicles);
            }
        }
        MonoBehaviour.print(GetCurrentTime(engine));
        MonoBehaviour.print(GetSuccess(engine));
        //print(GetFilePath(engine));
        //print(GetFilePath(engine));
        //print(GetVehicle(engine));
    }
}
