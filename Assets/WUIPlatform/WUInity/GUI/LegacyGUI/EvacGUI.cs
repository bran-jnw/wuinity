using UnityEngine;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string totalPop, maxCars, maxCarsProb, minHousehold, maxHousehold, walkingDistMod, walkSpeedMin, walkSpeedMax, walkSpeedMod, evacOrderTime;
        bool evacMenuDirty = true;        

        void EvacMenu()
        {
            PopulationInput popIn = WUIEngine.INPUT.Population;
            MacroHouseholdSimInput macroIn = WUIEngine.INPUT.Pedestrian.macroHouseholdSimInput;
            EvacuationInput evacIn = WUIEngine.INPUT.Evacuation;

            if (evacMenuDirty)
            {
                evacMenuDirty = false;
                maxCars = popIn.maxCars.ToString();
                maxCarsProb = popIn.maxCarsChance.ToString();
                minHousehold = popIn.minHouseholdSize.ToString();
                maxHousehold = popIn.maxHouseholdSize.ToString();
                walkSpeedMin = macroIn.walkingSpeedMinMax.X.ToString();
                walkSpeedMax = macroIn.walkingSpeedMinMax.Y.ToString();
                walkSpeedMod = macroIn.walkingSpeedModifier.ToString();
                walkingDistMod = macroIn.walkingDistanceModifier.ToString();
                evacOrderTime = evacIn.EvacuationOrderStart.ToString();

            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int buttonColumnStart = 140;

            //
            popIn.allowMoreThanOneCar = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), popIn.allowMoreThanOneCar, "Allow more than one car");
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
        }

        void ParseEvacInput()
        {
            if (evacMenuDirty)
            {
                return;
            }

            PopulationInput popIn = WUIEngine.INPUT.Population;
            MacroHouseholdSimInput macroIn = WUIEngine.INPUT.Pedestrian.macroHouseholdSimInput;
            EvacuationInput evacIn = WUIEngine.INPUT.Evacuation;

            int.TryParse(maxCars, out popIn.maxCars);
            float.TryParse(maxCarsProb, out popIn.maxCarsChance);
            int.TryParse(minHousehold, out popIn.minHouseholdSize);
            int.TryParse(maxHousehold, out popIn.maxHouseholdSize);
            float.TryParse(walkSpeedMin, out macroIn.walkingSpeedMinMax.X);
            float.TryParse(walkSpeedMax, out macroIn.walkingSpeedMinMax.Y);
            float.TryParse(walkSpeedMod, out macroIn.walkingSpeedModifier);
            float.TryParse(walkingDistMod, out macroIn.walkingDistanceModifier);
            float.TryParse(evacOrderTime, out evacIn.EvacuationOrderStart);
        }
    }
}
