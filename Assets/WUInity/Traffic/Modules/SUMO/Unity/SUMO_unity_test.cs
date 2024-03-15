using LIBSUMO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using WUInity.Traffic;

public class SUMO_unity_test : MonoBehaviour
{
    [SerializeField] float simTime = 600.0f;
    [SerializeField] float simTimeScale = 1.0f;
    [SerializeField] bool unlimitedSUMOUpdates = false;
    [SerializeField] bool smoothMovement = true;
    [SerializeField] GameObject carModel;
    SUMOCar controlledCar = null;
    bool simIsRunning = false;
    Dictionary<string, SUMOCar> cars = new Dictionary<string, SUMOCar>();
    [SerializeField] bool pauseSumo;
     
    System.Threading.Thread sumoThread;
    System.Object ThreadLock = new System.Object();
    ThreadController threadController;

    // Start is called before the first frame update
    void Start()
    {       
        sumoThread = new System.Threading.Thread(new ThreadStart(SumoStart));
        sumoThread.Start();        
    }

    private void OnDestroy()
    {
        KillSUMO();
    }

    void KillSUMO()
    {
        if(threadController != null)
        {
            print("SUMO thread should be killed.");
            threadController.killSUMO = true;
        }        
    }

    void SumoStart()
    {
        print("Loading SUMO.");
        LIBSUMO.Simulation.start(new StringVector(new String[] { "sumo", "-c", "D:\\ITSC2020_CAV_impact\\Urban\\Simulations\\Base\\DCC_simulation.sumo.cfg" }));
        print("SUMO started.");
        //LIBSUMO.Simulation.start(new StringVector(new String[] { "sumo", "-c", "D:\\sumo_test\\simulation.sumocfg"}));       
        simIsRunning = Simulation.isLoaded();
        if (simIsRunning)
        {
            simTime = (float)Simulation.getEndTime() < 0 ? simTime : (float)Simulation.getEndTime();
        }
        threadController = new ThreadController();
        ThreadedLoop(threadController);
    }

    // Update is called once per frame
    void Update()
    {
        if(simIsRunning)
        {
            SimLoop();
        }        
    }
    void UpdateVisuals(float lerpValue)
    {
        //if(activeCars != null)
        //{
            //lock (ThreadLock)
            //{
            if(cars.Count > 0)
            {
                SUMOCar[] c;
                lock (ThreadLock)
                {
                    c = cars.Values.ToArray();
                }
                
                for (int j = 0; j < c.Length; j++)
                {
                    SUMOCar car = c[j];
                    if (!car.IsDirectControlled() && car.IsActive())
                    {
                        //car.UpdateVisualPosition(1.0f);//lerpValue);
                    }
                }
            }   
                
            //}
        //}       
        
    }

    float nextUpdate = 0.0f;
    StringVector activeCars;
    void SimLoop()
    {           
        if(nextUpdate <= 0.0f)
        {
            //UpdateLoop();            
        }

        nextUpdate -= Time.deltaTime * simTimeScale;

        if(smoothMovement)
        {
            float lerpValue = (deltaTime - nextUpdate) / deltaTime;
            UpdateVisuals(lerpValue);
        }                 
    }

    void ThreadedLoop(ThreadController threadController)
    {
        ThreadController tC = threadController;

        while(UpdateLoop(tC))
        {
        }
        print("SUMO thread is done.");
    }

    float deltaTime;
    bool UpdateLoop(ThreadController tC)
    {
        bool doWork = true;        
        if(pauseSumo || nextUpdate > 0f)
        {
            doWork = false;
        }


        if(doWork || unlimitedSUMOUpdates)
        {
            Simulation.step();
            deltaTime = (float)Simulation.getDeltaT();
            nextUpdate = deltaTime; //seconds / 1000.0f;
            float currentSimTime = Simulation.getCurrentTime() / 1000.0f; // milliseconds

            if (controlledCar != null)
            {
                //TODO: set actual position and rotation
                //https://sumo.dlr.de/docs/TraCI/Change_Vehicle_State.html#move_to_xy_0xb4
                Vehicle.moveToXY(controlledCar.GetVehicleID(), "", -1, 0.0, 0.0, 0.0, 2);
            }

            //save injected vehicles
            if (Simulation.getDepartedNumber() > 0)
            {
                StringVector addedCars = Simulation.getDepartedIDList();
                lock(ThreadLock)
                {
                    for (int j = 0; j < addedCars.Count; j++)
                    {
                        string id = addedCars[j];
                        //cars.Add(id, new SUMO_car(id, Vehicle.getPosition(id), Vehicle.getAngle(id)));
                    }
                }                
            }

            //update positions
            activeCars = Vehicle.getIDList();
            for (int j = 0; j < activeCars.Count; j++)
            {
                string id = activeCars[j];
                SUMOCar car;
                bool found = cars.TryGetValue(id, out car);
                if (found && !car.IsDirectControlled())
                {
                    car.SetPosRot(Vehicle.getPosition(id), (float)Vehicle.getAngle(id));
                }
            }

            //set cars as inactive if arrived, have to be done after update of positions
            if (Simulation.getArrivedNumber() > 0)
            {
                StringVector removedCars = Simulation.getArrivedIDList();
                for (int j = 0; j < removedCars.Count; j++)
                {
                    SUMOCar car;
                    if (cars.TryGetValue(removedCars[j], out car))
                    {
                        car.Arrive();
                        //print("Time " + currentSimTime + ", car ID " + car.GetVehicleID() + " arrived.");
                    }
                }
            }

            if(currentSimTime >= simTime)
            {
                tC.killSUMO = true;
            }
        }        

        if (tC.killSUMO)
        {
            Simulation.close();
            simIsRunning = false;
            print("SUMO is closed.");
        }

        return simIsRunning;
    }

    uint vehiclerCounter = 0;
    public void InjectVehicle(System.Numerics.Vector2 start, System.Numerics.Vector2 goal, uint numberOfPeople)
    {
        string id = "car_" + vehiclerCounter;
        vehiclerCounter++;

        TraCIRoadPosition startRoad =  LIBSUMO.Simulation.convertRoad(start.X, start.Y, true);
        TraCIRoadPosition goalRoad = LIBSUMO.Simulation.convertRoad(goal.X, goal.Y, true);

        //TODO: check if route exists, if not create new one

        //https://sumo.dlr.de/docs/Simulation/Routing.html#travel-time_values_for_routing
        TraCIStage route = Simulation.findRoute(startRoad.edgeID, goalRoad.edgeID);
        string routeID = id + "_route";

        //TODO: save route for later checks
        Route.add(routeID, route.edges);
        Vehicle.add(id, routeID);
    }

    void CloseRoad(string edgeID)
    {
        //Rerouter.
    }
}
