using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Itinero;
using Itinero.Osm.Vehicles;

namespace WUInity
{
    public class MacroTrafficVerification
    {
        public static void RunTrafficVerificationTests()
        {
            TrafficInput trafficOptions = WUInity.WUINITY_IN.traffic;
            WUInityInput wuinityOptions = WUInity.WUINITY_IN;

            //general settings
            trafficOptions.visibilityAffectsSpeed = false;
            TrafficInjection[] trafficInjections = new TrafficInjection[1];
            trafficOptions.stallSpeed = 5f;
            trafficOptions.backGroundDensityMinMax = Vector2.zero;

            //test 1a
            wuinityOptions.simName = "verification_test_1a";
            Vector2D startPos = new Vector2D(0.0000, 0.0000); //start at middle of cross, big road
            Vector2D endPos = new Vector2D(0.0000, 0.0090009); //end at end of primary part 1
            int cars = 1;
            trafficInjections[0].cars = cars;
            trafficInjections[0].startPos = startPos;
            trafficInjections[0].endPos = endPos;
            RunTrafficVerificationSimulation(trafficInjections);

            //test 1b
            wuinityOptions.simName = "verification_test_1b";
            cars = 1;
            startPos = new Vector2D(0.0000, -0.0090009); //start at west residential     
            endPos = new Vector2D(0.0000, 0.0090009); //end at end of primary part 1
            trafficInjections[0].cars = cars;
            trafficInjections[0].startPos = startPos;
            trafficInjections[0].endPos = endPos;
            RunTrafficVerificationSimulation(trafficInjections);

            //test 2
            wuinityOptions.simName= "verification_test_2";
            cars = 1;
            startPos = new Vector2D(0.0000, 0.0000); //start at middle   
            endPos = new Vector2D(0.0000, 0.0090009); //end at end of primary part 1
            Vector2 oldDens = trafficOptions.backGroundDensityMinMax;
            trafficOptions.backGroundDensityMinMax = new Vector2(37.5f, 37.5f); //50% load 75/2 = 37.5
            trafficInjections[0].cars = cars;
            trafficInjections[0].startPos = startPos;
            trafficInjections[0].endPos = endPos;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset some options
            trafficOptions.backGroundDensityMinMax = oldDens;

            //test 3
            startPos = new Vector2D(0.0000, -0.0090009); //start at west residential     
            endPos = new Vector2D(0.0000, 0.0090009); //end at end of primary part 1 
            int oldLanes = trafficOptions.roadTypes.roadData[4].lanes;
            trafficOptions.roadTypes.roadData[4].lanes = 2;
            for (int i = 0; i < 5; i++)
            {
                float carDensity = (i * 12.5f); //residential has capacity of 50 cars/lane/km
                cars = Mathf.RoundToInt(carDensity); //1 lane, 1000 m road
                if (i == 0)
                {
                    cars = 1;
                }
                wuinityOptions.simName= "verification_test_3_dens_" + carDensity;
                trafficInjections[0].cars = cars;
                trafficInjections[0].startPos = startPos;
                trafficInjections[0].endPos = endPos;
                RunTrafficVerificationSimulation(trafficInjections);
            }
            //reset
            trafficOptions.roadTypes.roadData[4].lanes = oldLanes;

            //test 4
            wuinityOptions.simName= "verification_test_4";
            //bool oldValue = trafficOptions.roadTypes.roadData[4].canBeReversed;
            //trafficOptions.roadTypes.roadData[4].canBeReversed = true;
            cars = 1;
            startPos = new Vector2D(0.0000, 0.0000); //start at middle   
            endPos = new Vector2D(0.0000, 0.0090009); //end at end of primary part 1 
            Traffic.MacroTrafficSim.ReverseLanes reverse = new Traffic.MacroTrafficSim.ReverseLanes(30f, 1000000f);
            Traffic.MacroTrafficSim.TrafficEvent[] events = new Traffic.MacroTrafficSim.TrafficEvent[1];
            events[0] = reverse;
            trafficInjections[0].cars = cars;
            trafficInjections[0].startPos = startPos;
            trafficInjections[0].endPos = endPos;
            RunTrafficVerificationSimulation(trafficInjections, events);
            //reset
            //trafficOptions.roadTypes.roadData[4].canBeReversed = oldValue;

            //test 5
            trafficInjections = new TrafficInjection[3];

            //point 1
            startPos = new Vector2D(0.0000, 0.0000); //start at middle of cross, big road
            endPos = new Vector2D(0.0000, 0.0270027); //end at the end of 3x1000m segments

            trafficInjections[0].startPos = startPos;
            trafficInjections[0].endPos = endPos;

            //point 2
            startPos = new Vector2D(0.0000, 0.0090009);
            trafficInjections[1].startPos = startPos;
            trafficInjections[1].endPos = endPos;

            //point 3
            startPos = new Vector2D(0.0000, 0.0180018);
            trafficInjections[2].startPos = startPos;
            trafficInjections[2].endPos = endPos;

            for (int i = 0; i < 5; i++)
            {
                cars = Mathf.RoundToInt(i * 18.75f); //1 lane, 1000 m road
                if (i == 0)
                {
                    cars = 1;
                }
                trafficInjections[0].cars = cars;
                trafficInjections[1].cars = cars;
                trafficInjections[2].cars = cars;

                wuinityOptions.simName= "verification_test_5_dens_" + cars;

                //run 5
                RunTrafficVerificationSimulation(trafficInjections);
            }

            //test 6
            RunDrivingInSmokeVerification();

            //test 7 same as test 1. rerun?
        }

        public static void RunDrivingInSmokeVerification()
        {
            TrafficInput trafficOptions = WUInity.WUINITY_IN.traffic;
            WUInityInput wuinityOptions = WUInity.WUINITY_IN;

            //modify speedlimit on primary since we are using the same osm data as the other ones that use 90 km/h
            float tempValue = trafficOptions.roadTypes.roadData[4].speedLimit;
            trafficOptions.roadTypes.roadData[4].speedLimit = 70.0f;

            Vector2D startPos = new Vector2D(0.0000, 0.0000); //.0045, 0.0009 is on the far end with small road
            Vector2D endPos = new Vector2D(0.0000, 0.0090009);

            TrafficInjection[] trafficInjections = new TrafficInjection[1];
            trafficInjections[0].startPos = startPos;
            trafficInjections[0].endPos = endPos;

            //car density change
            for (int i = 0; i < 5; i++)
            {
                float carDensity = (i * 18.75f);
                int cars = Mathf.RoundToInt(carDensity); //1 lane, 1500 m road
                if (i == 0)
                {
                    cars = 1;
                }
                trafficInjections[0].cars = cars;
                //smoke density change
                for (int j = 0; j < 5; j++)
                {
                    float D_L = j * 0.05f;
                    if (D_L == 0.0f)
                    {
                        trafficOptions.visibilityAffectsSpeed = false;
                    }
                    else
                    {
                        trafficOptions.visibilityAffectsSpeed = true;
                    }
                    trafficOptions.opticalDensity = D_L;

                    wuinityOptions.simName= "verification_test_6_dens_" + carDensity + "_D_L_" + D_L.ToString("0.00");

                    RunTrafficVerificationSimulation(trafficInjections);
                }
            }

            //reset changes
            trafficOptions.roadTypes.roadData[4].speedLimit = tempValue;
        }

        public static void RunTrafficVerificationSimulation(TrafficInjection[] trafficInjections, Traffic.MacroTrafficSim.TrafficEvent[] events = null)
        {
            TrafficInput trafficOptions = WUInity.WUINITY_IN.traffic;
            WUInityInput wuinityOptions = WUInity.WUINITY_IN;

            WUInity.WUINITY_SIM.LoadItineroDatabase();

            Traffic.MacroTrafficSim traffic = new Traffic.MacroTrafficSim(null); //WUInity.WUINITY_SIM.routeCreator

            if (events != null)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    traffic.InsertNewTrafficEvent(events[i]);
                }
            }

            Router router = new Router(WUInity.WUINITY_SIM.GetRouterDb());
            Itinero.Profiles.Profile p = null;
            if (trafficOptions.routeChoice == TrafficInput.RouteChoice.Closest)
            {
                p = Vehicle.Car.Shortest();
            }
            else if (trafficOptions.routeChoice == TrafficInput.RouteChoice.Fastest)
            {
                p = Vehicle.Car.Fastest();
            }
            else if (trafficOptions.routeChoice == TrafficInput.RouteChoice.ForceMap)
            {
                p = Vehicle.Car.Fastest();
            }

            //inject all the traffic
            for (int i = 0; i < trafficInjections.Length; i++)
            {
                Vector2D startPos = trafficInjections[i].startPos;
                Vector2D endPos = trafficInjections[i].endPos;

                RouterPoint start = router.Resolve(p, (float)startPos.x, (float)startPos.y, 50f);
                RouterPoint end = router.Resolve(p, (float)endPos.x, (float)endPos.y, 300f);

                Route route = router.Calculate(p, start.Latitude, start.Longitude, end.Latitude, end.Longitude);
                EvacuationGoal goal = new EvacuationGoal();
                RouteData routeData = new RouteData(route, goal);

                int carsToInject = trafficInjections[i].cars;

                for (int j = 0; j < carsToInject; j++)
                {
                    traffic.InsertNewCar(routeData, 1);
                }
            }

            float time = 0.0f;

            while (!traffic.EvacComplete())
            {
                traffic.AdvanceTrafficSimulation(wuinityOptions.deltaTime, time);
                time += wuinityOptions.deltaTime;
            }
            traffic.SaveToFile(0);

            //force garbage collection
            System.GC.Collect();
        }
    }
}

