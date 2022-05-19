using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Itinero;
using Itinero.Osm.Vehicles;

namespace WUInity.Traffic
{
    public static class MacroTrafficVerification
    {
        public static void RunTrafficVerificationTests()
        {
            SaveLoadWUI.LoadInput("traffic_verification");
            TrafficInput trafficOptions = WUInity.INPUT.traffic;
            WUInityInput wuinityOptions = WUInity.INPUT;

            //Custom road types index start at 20 and goes to 24, residential is 11, primary is 4
            //N-E road is custom 0, S-E is custom 1
            EvacuationGoal node2 = new EvacuationGoal("Node 2", new Vector2D(0.0, -0.0090009), Color.white);
            EvacuationGoal node4 = new EvacuationGoal("Node 4", new Vector2D(0.0, 0.0), Color.white);
            EvacuationGoal node5 = new EvacuationGoal("Node 5", new Vector2D(0.0, 0.0090009), Color.white); 
            EvacuationGoal node6 = new EvacuationGoal("Node 6", new Vector2D(0.0, 0.0180018), Color.white);
            EvacuationGoal node7 = new EvacuationGoal("Node 7", new Vector2D(0.0, 0.0270027), Color.white);
            EvacuationGoal node8 = new EvacuationGoal("Node 8", new Vector2D(0.00450045, 0.00450045), Color.white);
            EvacuationGoal node9 = new EvacuationGoal("Node 9", new Vector2D(-0.00450045, 0.01350135), Color.white);

            EvacuationGoal node10 = new EvacuationGoal("Node 10", new Vector2D(0.0, 0.0360036), Color.white);
            EvacuationGoal node11 = new EvacuationGoal("Node 11", new Vector2D(0.0, 0.0450045), Color.white);
            EvacuationGoal node12 = new EvacuationGoal("Node 12", new Vector2D(0.0090009, 0.0270027), Color.white);
            EvacuationGoal node13 = new EvacuationGoal("Node 13", new Vector2D(0.0090009, 0.0360036), Color.white);
            EvacuationGoal node14 = new EvacuationGoal("Node 14", new Vector2D(0.0090009, 0.0450045), Color.white);

            EvacuationGoal[] nodes = new EvacuationGoal[] { node2, node4, node5, node6, node7, node8, node9, node10, node11, node12, node13, node14 };

            //general settings
            trafficOptions.visibilityAffectsSpeed = false;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest; //this will change is some cases
            trafficOptions.stallSpeed = 1f;
            trafficOptions.backGroundDensityMinMax = Vector2.zero;

            WUInity.SIM_DATA.EvacuationGoals= new EvacuationGoal[1];
            SimpleTrafficInjection[] trafficInjections = new SimpleTrafficInjection[1];            

            //test 1a
            wuinityOptions.simName = "T1a";                        
            trafficInjections[0] = new SimpleTrafficInjection(1, node2.latLong, node4);
            WUInity.SIM_DATA.EvacuationGoals[0] = node4;
            float oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            for (int i = 0; i < 5; i++)
            {
                trafficOptions.roadTypes.roadData[11].speedLimit = 30f + i * 20f;
                wuinityOptions.simName = "T1a_" +(int)trafficOptions.roadTypes.roadData[11].speedLimit;
                RunTrafficVerificationSimulation(trafficInjections);
                //reset
                ResetNodes(nodes);
            }
            trafficOptions.roadTypes.roadData[11].speedLimit = oldSpeedLimit;

            //test 1b
            wuinityOptions.simName = "T1b";
            trafficInjections[0] = new SimpleTrafficInjection(1, node2.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            ResetNodes(nodes);

            //test 2
            wuinityOptions.simName= "T2";
            trafficInjections[0] = new SimpleTrafficInjection(1, node4.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            Vector2 oldDens = trafficOptions.backGroundDensityMinMax;
            trafficOptions.backGroundDensityMinMax = new Vector2(37.5f, 37.5f); //50% load 75/2 = 37.5            
            RunTrafficVerificationSimulation(trafficInjections);
            //reset some options
            trafficOptions.backGroundDensityMinMax = oldDens;
            ResetNodes(nodes);

            //test 3
            trafficInjections[0] = new SimpleTrafficInjection(1, node2.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            //save for later
            int oldLanes = trafficOptions.roadTypes.roadData[4].lanes;
            //residential higher speed
            oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            trafficOptions.roadTypes.roadData[11].speedLimit = 90f;
            //more lanes
            trafficOptions.roadTypes.roadData[4].lanes = 2;
            for (int i = 0; i < 5; i++)
            {
                float carDensity = (i * 12.5f); //residential has capacity of 50 cars/lane/km
                int cars = Mathf.RoundToInt(carDensity); //1 lane, 1000 m road
                if (i == 0)
                {
                    cars = 1;
                }
                wuinityOptions.simName= "T3_dens_" + carDensity;
                trafficInjections[0].cars = cars;                
                RunTrafficVerificationSimulation(trafficInjections);
                ResetNodes(nodes);
            }
            //reset back to old lanes
            trafficOptions.roadTypes.roadData[11].speedLimit = oldSpeedLimit;
            trafficOptions.roadTypes.roadData[4].lanes = oldLanes;
            ResetNodes(nodes);

            //test 4, speed/density
            WUInity.SIM_DATA.EvacuationGoals[0] = node7;
            trafficInjections = new SimpleTrafficInjection[3];
            //point 1
            trafficInjections[0] = new SimpleTrafficInjection(1, node4.latLong, node7);
            //point 2
            trafficInjections[1] = new SimpleTrafficInjection(1, node5.latLong, node7);
            //point 3
            trafficInjections[2] = new SimpleTrafficInjection(1, node6.latLong, node7);
            //save for later
            oldSpeedLimit = trafficOptions.roadTypes.roadData[4].speedLimit;
            //more lanes
            trafficOptions.roadTypes.roadData[4].speedLimit = 70f;

            for (int i = 0; i < 5; i++)
            {
                int cars = Mathf.RoundToInt(i * 18.75f); //1 lane, 1000 m road
                if (i == 0)
                {
                    cars = 1;
                }
                trafficInjections[0].cars = cars;
                trafficInjections[1].cars = cars;
                trafficInjections[2].cars = cars;
                wuinityOptions.simName= "T4_dens_" + cars;
                //run
                RunTrafficVerificationSimulation(trafficInjections);
                ResetNodes(nodes);
            }
            //reset
            trafficOptions.roadTypes.roadData[4].speedLimit = oldSpeedLimit;
            ResetNodes(nodes);

            //test 5, driving in smoke
            RunDrivingInSmokeVerification();

            //T6
            //car density change
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            trafficInjections = new SimpleTrafficInjection[1];
            trafficInjections[0] = new SimpleTrafficInjection(1, node4.latLong, node5);
            //save for later
            oldSpeedLimit = trafficOptions.roadTypes.roadData[4].speedLimit;
            trafficOptions.roadTypes.roadData[4].speedLimit = 70f;
            for (int i = 0; i < 5; i++)
            {
                int cars = Mathf.RoundToInt(i * 18.75f); //1 lane, 1000 m road
                if (i == 0)
                {
                    cars = 1;
                }
                trafficInjections[0].cars = cars;
                wuinityOptions.simName= "T6_dens_" + cars;
                //run
                RunTrafficVerificationSimulation(trafficInjections);
                ResetNodes(nodes);
            }
            //reset
            trafficOptions.roadTypes.roadData[4].speedLimit = oldSpeedLimit;
            ResetNodes(nodes);

            //T7, change residential settings 
            wuinityOptions.simName = "T7";
            trafficInjections = new SimpleTrafficInjection[1];
            trafficInjections[0] = new SimpleTrafficInjection(2, node4.latLong, node9);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[2];
            WUInity.SIM_DATA.EvacuationGoals[0] = node8;
            WUInity.SIM_DATA.EvacuationGoals[1] = node9;
            oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            trafficOptions.roadTypes.roadData[11].speedLimit = 90f;
            trafficOptions.roadTypes.roadData[20].speedLimit = 90f;
            trafficOptions.roadTypes.roadData[21].speedLimit = 90f;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.EvacGroup;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            trafficOptions.roadTypes.roadData[11].speedLimit = oldSpeedLimit;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            ResetNodes(nodes);

            //T8 - overtaking, not possible

            //T9 - acceleration, "not possible" (not implemented)

            //T10 - road accident
            wuinityOptions.simName = "T10_stall_speed_1";
            trafficInjections[0] = new SimpleTrafficInjection(1, node4.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[1];
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            Traffic.MacroTrafficSim.TrafficAccident accident = new Traffic.MacroTrafficSim.TrafficAccident(10f, 1000000f);
            Traffic.MacroTrafficSim.TrafficEvent[] events = new Traffic.MacroTrafficSim.TrafficEvent[1];
            events[0] = accident;
            RunTrafficVerificationSimulation(trafficInjections, events);
            //reset
            accident.ResetEvent();
            ResetNodes(nodes);
            //stall speed 0
            float oldStallSpeed = wuinityOptions.traffic.stallSpeed;
            float oldMaxSimTime = wuinityOptions.maxSimTime;
            wuinityOptions.traffic.stallSpeed = 0f;
            wuinityOptions.simName = "T10_stall_speed_0";
            wuinityOptions.maxSimTime = 3600.0f;
            RunTrafficVerificationSimulation(trafficInjections, events);
            ResetNodes(nodes);
            wuinityOptions.traffic.stallSpeed = oldStallSpeed;
            wuinityOptions.maxSimTime = oldMaxSimTime;

            //T11 - intersection
            wuinityOptions.simName = "T11";
            trafficInjections[0] = new SimpleTrafficInjection(1, node2.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            trafficOptions.roadTypes.roadData[11].speedLimit = 90f;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            trafficOptions.roadTypes.roadData[11].speedLimit = oldSpeedLimit;
            ResetNodes(nodes);

            //T12 - forced destination
            wuinityOptions.simName = "T12";
            trafficInjections[0] = new SimpleTrafficInjection(1, node5.latLong, node7);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[2];
            WUInity.SIM_DATA.EvacuationGoals[0] = node4;
            WUInity.SIM_DATA.EvacuationGoals[1] = node7;
            oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.EvacGroup;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            ResetNodes(nodes);

            //T13 - destination choice in traffic
            wuinityOptions.simName = "T13_fastest";
            trafficInjections[0] = new SimpleTrafficInjection(1, node7.latLong, node10);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[2];
            WUInity.SIM_DATA.EvacuationGoals[0] = node10;
            WUInity.SIM_DATA.EvacuationGoals[1] = node13;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            RunTrafficVerificationSimulation(trafficInjections);
            wuinityOptions.simName = "T13_closest";
            ResetNodes(nodes);
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Closest;            
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            ResetNodes(nodes);

            //T14 - route choice in traffic
            wuinityOptions.simName = "T14_fastest";
            trafficInjections[0] = new SimpleTrafficInjection(1, node7.latLong, node11);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[1];
            WUInity.SIM_DATA.EvacuationGoals[0] = node11;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            RunTrafficVerificationSimulation(trafficInjections);
            wuinityOptions.simName = "T14_closest";
            ResetNodes(nodes);
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Closest;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            ResetNodes(nodes);

            //T15
            wuinityOptions.simName = "T15a";
            trafficInjections[0] = new SimpleTrafficInjection(2, node4.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[1];
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;            
            RunTrafficVerificationSimulation(trafficInjections);
            wuinityOptions.simName = "T15b";
            ResetNodes(nodes);
            trafficInjections[0] = new SimpleTrafficInjection(50, node4.latLong, node5);
            RunTrafficVerificationSimulation(trafficInjections);
            wuinityOptions.simName = "T15c";
            ResetNodes(nodes);
            trafficInjections[0] = new SimpleTrafficInjection(100, node4.latLong, node5);
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            ResetNodes(nodes);

            //WT1 - route loss            

            //WT 2, lane reversal
            trafficInjections[0] = new SimpleTrafficInjection(1, node4.latLong, node5);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[1];
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;
            Traffic.MacroTrafficSim.ReverseLanes reverse = new Traffic.MacroTrafficSim.ReverseLanes(30f, 1000000f);
            events = new Traffic.MacroTrafficSim.TrafficEvent[1];
            events[0] = reverse;
            for (int i = 0; i < 5; i++)
            {
                int cars = Mathf.RoundToInt(i * 18.75f); //1 lane, 1000 m road
                if (i == 0)
                {
                    cars = 1;
                }
                trafficInjections[0].cars = cars;
                wuinityOptions.simName = "WT2_dens_" + cars;
                //run
                RunTrafficVerificationSimulation(trafficInjections, events);
                ResetNodes(nodes);
                reverse.ResetEvent();
            }

            //WT3 - destination loss
            wuinityOptions.simName = "WT3";
            trafficInjections[0] = new SimpleTrafficInjection(1, node2.latLong, node8);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[2];
            WUInity.SIM_DATA.EvacuationGoals[0] = node8;
            WUInity.SIM_DATA.EvacuationGoals[1] = node9;
            oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            trafficOptions.roadTypes.roadData[11].speedLimit = 90f;
            trafficOptions.roadTypes.roadData[20].speedLimit = 90f;
            trafficOptions.roadTypes.roadData[21].speedLimit = 90f;
            BlockGoalEvent destLoss = new BlockGoalEvent(60f, 0);
            BlockGoalEvent[] bGEs = new BlockGoalEvent[1];
            bGEs[0] = destLoss;
            events[0] = accident;
            RunTrafficVerificationSimulation(trafficInjections, null, bGEs);
            //reset
            trafficOptions.roadTypes.roadData[11].speedLimit = oldSpeedLimit;
            ResetNodes(nodes);

            //WT4 - full refuge
            wuinityOptions.simName = "WT4";
            trafficOptions.routeChoice = TrafficInput.RouteChoice.EvacGroup;
            trafficInjections[0] = new SimpleTrafficInjection(2, node2.latLong, node8);
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[2];
            node8.maxCars = 1;            
            node8.goalType = EvacGoalType.Refugee;
            WUInity.SIM_DATA.EvacuationGoals[0] = node8;
            WUInity.SIM_DATA.EvacuationGoals[1] = node9;
            oldSpeedLimit = trafficOptions.roadTypes.roadData[11].speedLimit;
            trafficOptions.roadTypes.roadData[11].speedLimit = 90f;
            trafficOptions.roadTypes.roadData[20].speedLimit = 90f;
            trafficOptions.roadTypes.roadData[21].speedLimit = 90f;
            RunTrafficVerificationSimulation(trafficInjections);
            //reset
            trafficOptions.roadTypes.roadData[11].speedLimit = oldSpeedLimit;
            trafficOptions.routeChoice = TrafficInput.RouteChoice.Fastest;
            node8.maxCars = -1;
            node8.goalType = EvacGoalType.Exit;
            ResetNodes(nodes);
        }

        static void RunDrivingInSmokeVerification()
        {
            TrafficInput trafficOptions = WUInity.INPUT.traffic;
            WUInityInput wuinityOptions = WUInity.INPUT;

            EvacuationGoal node4 = new EvacuationGoal("Node 4", new Vector2D(0.0, 0.0), Color.white);
            EvacuationGoal node5 = new EvacuationGoal("Node 5", new Vector2D(0.0, 0.0090009), Color.white);

            //modify speedlimit on primary since we are using the same osm data as the other ones that use 90 km/h
            float tempValue = trafficOptions.roadTypes.roadData[4].speedLimit;
            trafficOptions.roadTypes.roadData[4].speedLimit = 70.0f;

            Vector2D startPos = node4.latLong;//.0045, 0.0009 is on the far end with small road
            WUInity.SIM_DATA.EvacuationGoals = new EvacuationGoal[1];
            WUInity.SIM_DATA.EvacuationGoals[0] = node5;

            SimpleTrafficInjection[] trafficInjections = new SimpleTrafficInjection[1];
            trafficInjections[0] = new SimpleTrafficInjection(1, startPos, node5);

            //car density change
            for (int i = 0; i < 5; i++)
            {
                float carDensity = (i * 18.75f);
                int cars = Mathf.RoundToInt(carDensity); //1 lane, 1000 m road
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

                    wuinityOptions.simName= "T5_dens_" + carDensity + "_D_L_" + D_L.ToString("0.00");

                    RunTrafficVerificationSimulation(trafficInjections);
                    node5.ResetPeopleAndCars();
                }
                node5.ResetPeopleAndCars();
            }

            //reset changes
            trafficOptions.roadTypes.roadData[4].speedLimit = tempValue;
            trafficOptions.opticalDensity = 0f;
            trafficOptions.visibilityAffectsSpeed = false;
        }

        static void ResetNodes(EvacuationGoal[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].ResetPeopleAndCars();
            }
        }

        static void RunTrafficVerificationSimulation(SimpleTrafficInjection[] trafficInjections, Traffic.MacroTrafficSim.TrafficEvent[] events = null, BlockGoalEvent[] blockGoalEvents = null)
        {
            TrafficInput trafficOptions = WUInity.INPUT.traffic;
            WUInityInput wuinityOptions = WUInity.INPUT;

            WUInity.SIM_DATA.LoadRouterDatabase();

            RouteCreator routeCreator = new RouteCreator();
            routeCreator.SetValidGoals();
            Traffic.MacroTrafficSim traffic = new Traffic.MacroTrafficSim(routeCreator); //WUInity.WUINITY_SIM.routeCreator
            WUInity.SIM.SetMacroTrafficSim(traffic);

            if (events != null)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    traffic.InsertNewTrafficEvent(events[i]);
                }
            }            

            Router router = new Router(WUInity.SIM_DATA.GetRouterDb());
            Itinero.Profiles.Profile p;
            if (trafficOptions.routeChoice == TrafficInput.RouteChoice.Closest)
            {
                p = Vehicle.Car.Shortest();
            }
            else if (trafficOptions.routeChoice == TrafficInput.RouteChoice.Fastest)
            {
                p = Vehicle.Car.Fastest();
            }
            else
            {
                p = Vehicle.Car.Fastest();
            }

            //inject all the traffic
            for (int i = 0; i < trafficInjections.Length; i++)
            {
                Vector2D startPos = trafficInjections[i].startPos;
                RouteData routeData = GetRoute(startPos, router, p, trafficInjections[i].desiredGoal);

                int carsToInject = trafficInjections[i].cars;

                for (int j = 0; j < carsToInject; j++)
                {
                    traffic.InsertNewCar(routeData, 1);
                }
            }

            float time = 0.0f;
            while (!traffic.EvacComplete() && time <= wuinityOptions.maxSimTime)
            {
                //check if we are losing goals
                if (blockGoalEvents != null)
                {
                    for (int i = 0; i < blockGoalEvents.Length; i++)
                    {
                        BlockGoalEvent bGE = blockGoalEvents[i];
                        if (time >= bGE.startTime && !bGE.triggered)
                        {
                            bGE.ApplyEffects();
                        }
                    }
                }

                traffic.AdvanceTrafficSimulation(wuinityOptions.deltaTime, time);
                time += wuinityOptions.deltaTime;

                bool allGoalsBlocked = true;
                for (int i = 0; i < WUInity.SIM_DATA.EvacuationGoals.Length; i++)
                {
                    if(!WUInity.SIM_DATA.EvacuationGoals[i].blocked)
                    {
                        allGoalsBlocked = false;
                        break;
                    }
                }

                if(allGoalsBlocked)
                {
                    WUInity.WUI_LOG("All goals blocked, aborting verification simulation " + wuinityOptions.simName);
                    break;
                }
            }
            traffic.SaveToFile(0);

            //force garbage collection
            System.GC.Collect();
        }

        static RouteData GetRoute(Vector2D startPos, Router router, Itinero.Profiles.Profile p, EvacuationGoal desiredGoal)
        {
            TrafficInput trafficOptions = WUInity.INPUT.traffic;
            RouteCollection rC = new RouteCollection(WUInity.SIM_DATA.EvacuationGoals.Length);
            for (int i = 0; i < WUInity.SIM_DATA.EvacuationGoals.Length; i++)
            {
                Vector2D endPos = WUInity.SIM_DATA.EvacuationGoals[i].latLong;

                RouterPoint start = router.Resolve(p, (float)startPos.x, (float)startPos.y, 100f);
                RouterPoint end = router.Resolve(p, (float)endPos.x, (float)endPos.y, 100f);

                Route route = router.Calculate(p, start.Latitude, start.Longitude, end.Latitude, end.Longitude);
                RouteData routeData = new RouteData(route, WUInity.SIM_DATA.EvacuationGoals[i]);
                rC.routes[i] = routeData;
            }

            if (trafficOptions.routeChoice == TrafficInput.RouteChoice.Closest)
            {
                rC.SelectClosestNonBlocked();
            }
            else if (trafficOptions.routeChoice == TrafficInput.RouteChoice.Fastest)
            {
                rC.SelectFastestNonBlocked();
            }
            else if(desiredGoal != null)
            {
                rC.SelectForcedNonBlocked(desiredGoal);
            }

            return rC.GetSelectedRoute();
        }
    }
}

