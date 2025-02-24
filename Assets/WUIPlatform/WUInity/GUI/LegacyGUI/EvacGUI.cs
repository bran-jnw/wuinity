using UnityEngine;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string totalPop, cellSize, maxCars, maxCarsProb, minHousehold, maxHousehold, walkingDistMod, walkSpeedMin, walkSpeedMax, walkSpeedMod, evacOrderTime;
        bool evacMenuDirty = true;        

        void EvacMenu()
        {
            EvacuationInput eO = WUIEngine.INPUT.Evacuation;
            if (evacMenuDirty)
            {
                evacMenuDirty = false;
                cellSize = eO.RouteCellSize.ToString();
                maxCars = eO.maxCars.ToString();
                maxCarsProb = eO.maxCarsChance.ToString();
                minHousehold = eO.minHouseholdSize.ToString();
                maxHousehold = eO.maxHouseholdSize.ToString();
                walkSpeedMin = eO.walkingSpeedMinMax.X.ToString();
                walkSpeedMax = eO.walkingSpeedMinMax.Y.ToString();
                walkSpeedMod = eO.walkingSpeedModifier.ToString();
                walkingDistMod = eO.walkingDistanceModifier.ToString();
                evacOrderTime = eO.EvacuationOrderStart.ToString();

            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int buttonColumnStart = 140;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cell size [m]");
            ++buttonIndex;
            cellSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), cellSize);
            ++buttonIndex;

            //
            eO.allowMoreThanOneCar = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), eO.allowMoreThanOneCar, "Allow more than one car");
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Max cars [-]");
            ++buttonIndex;
            maxCars = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), maxCars);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Probability for max cars");
            ++buttonIndex;
            maxCarsProb = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), maxCarsProb);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Min. persons per household");
            ++buttonIndex;
            minHousehold = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), minHousehold);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Max. persons per household");
            ++buttonIndex;
            maxHousehold = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), maxHousehold);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Min. walking speed");
            ++buttonIndex;
            walkSpeedMin = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkSpeedMin);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Max. walking speed");
            ++buttonIndex;
            walkSpeedMax = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkSpeedMax);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Walking speed mod.");
            ++buttonIndex;
            walkSpeedMod = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkSpeedMod);
            ++buttonIndex;
            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Walking distance mod.");
            ++buttonIndex;
            walkingDistMod = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), walkingDistMod);
            ++buttonIndex;

            //
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Evacuation order [time after fire]");
            ++buttonIndex;
            evacOrderTime = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), evacOrderTime);
            ++buttonIndex;            

            if (!WUInityEngine.INSTANCE.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit evac group"))
                {
                    WUInityEngine.INSTANCE.StartPainter(Painter.PaintMode.EvacGroup);
                }
                ++buttonIndex;                
            }
            else
            {
                for (int i = 0; i < WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGroups.Length; i++)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), WUIEngine.RUNTIME_DATA.Evacuation.EvacuationGroups[i].Name))
                    {
                        WUInityEngine.Painter.SetEvacGroupColor(i);
                    }
                    ++buttonIndex;
                }

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Finish editing"))
                {
                    WUInityEngine.INSTANCE.StopPainter();
                }
                ++buttonIndex;
            }

            ++buttonIndex;
            if (WUInityEngine.INSTANCE.DeveloperMode)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run evac verification"))
                {
                    WUIPlatform.Pedestrian.MacroHumanVerification.RunVerification();
                }
                ++buttonIndex;
            }
        }

        void ParseEvacInput()
        {
            if (evacMenuDirty)
            {
                return;
            }

            EvacuationInput eO = WUIEngine.INPUT.Evacuation;

            float.TryParse(cellSize, out eO.RouteCellSize);
            int.TryParse(maxCars, out eO.maxCars);
            float.TryParse(maxCarsProb, out eO.maxCarsChance);
            int.TryParse(minHousehold, out eO.minHouseholdSize);
            int.TryParse(maxHousehold, out eO.maxHouseholdSize);
            float.TryParse(walkSpeedMin, out eO.walkingSpeedMinMax.X);
            float.TryParse(walkSpeedMax, out eO.walkingSpeedMinMax.Y);
            float.TryParse(walkSpeedMod, out eO.walkingSpeedModifier);
            float.TryParse(walkingDistMod, out eO.walkingDistanceModifier);
            float.TryParse(evacOrderTime, out eO.EvacuationOrderStart);
        }
    }
}
