using UnityEngine;
using System;

public class SUMO_test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsSumoLoaded(0))
        {
            LIBSUMO.Simulation.close();
            IsSumoLoaded(1);
        }

        LIBSUMO.Simulation.start(new LIBSUMO.StringVector(new String[] { "sumo", "-c", "E:\\_UNITY\\WUInity\\wui-nity\\Example\\sumo\\rox.sumocfg", "-b", 0f.ToString(), "-e", 1000f.ToString() }));        

        if (IsSumoLoaded(2))
        {
            for (int i = 0; i < 100; i++)
            {
                LIBSUMO.Simulation.step();
            }

            LIBSUMO.Simulation.close();
            IsSumoLoaded(3);
        }
    }

    private bool IsSumoLoaded(int nr)
    {
        bool success = LIBSUMO.Simulation.isLoaded();
        if (success)
        {
            print("Sumo is loaded. " + nr);
        }
        else
        {
            print("Sumo is NOT loaded. " + nr);
        }

        return success;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
