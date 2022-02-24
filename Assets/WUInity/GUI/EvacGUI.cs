using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string totalPop, cellSize, maxCars, maxCarsProb, minHousehold, maxHousehold, walkingDistMod, walkSpeedMin, walkSpeedMax, walkSpeedMod, evacOrderTime;
        bool evacInputDirty = true;        

        void EvacMenu()
        {
            EvacInput eO = WUInity.INPUT.evac;
            if (evacInputDirty)
            {
                evacInputDirty = false;
                cellSize = eO.routeCellSize.ToString();
                maxCars = eO.maxCars.ToString();
                maxCarsProb = eO.maxCarsChance.ToString();
                minHousehold = eO.minHouseholdSize.ToString();
                maxHousehold = eO.maxHouseholdSize.ToString();
                walkSpeedMin = eO.walkingSpeedMinMax.x.ToString();
                walkSpeedMax = eO.walkingSpeedMinMax.y.ToString();
                walkSpeedMod = eO.walkingSpeedModifier.ToString();
                walkingDistMod = eO.walkingDistanceModifier.ToString();
                evacOrderTime = eO.evacuationOrderStart.ToString();

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

            if (WUInity.INSTANCE.developerMode)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run evac verification"))
                {
                    Evac.MacroHumanVerification.RunVerification();
                }
                ++buttonIndex;
            }

            if (WUInity.INSTANCE.IsPainterActive())
            {
                for (int i = 0; i < WUInity.INPUT.evac.evacGroups.Length; i++)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Define group " + (i + 1)))
                    {
                        WUInity.PAINTER.SetEvacGroupColor(i);
                    }
                    ++buttonIndex;
                }

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop editing"))
                {
                    WUInity.INSTANCE.StopPainter();
                }
                ++buttonIndex;
            }
            else
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit evac group"))
                {
                    WUInity.INSTANCE.StartPainter(Painter.PaintMode.EvacGroup);
                }
                ++buttonIndex;
            }
        }

        void ParseEvacInput()
        {
            if (evacInputDirty)
            {
                return;
            }

            EvacInput eO = WUInity.INPUT.evac;

            float.TryParse(cellSize, out eO.routeCellSize);
            int.TryParse(maxCars, out eO.maxCars);
            float.TryParse(maxCarsProb, out eO.maxCarsChance);
            int.TryParse(minHousehold, out eO.minHouseholdSize);
            int.TryParse(maxHousehold, out eO.maxHouseholdSize);
            float.TryParse(walkSpeedMin, out eO.walkingSpeedMinMax.x);
            float.TryParse(walkSpeedMax, out eO.walkingSpeedMinMax.y);
            float.TryParse(walkSpeedMod, out eO.walkingSpeedModifier);
            float.TryParse(walkingDistMod, out eO.walkingDistanceModifier);
            float.TryParse(evacOrderTime, out eO.evacuationOrderStart);
        }
    }
}
