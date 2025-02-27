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
                maxCars = popIn.MaxCars.ToString();
                maxCarsProb = popIn.MaxCarsProbability.ToString();
                minHousehold = popIn.MinHouseholdSize.ToString();
                maxHousehold = popIn.MaxHouseholdSize.ToString();
                walkSpeedMin = macroIn.WalkingSpeedMinMax.X.ToString();
                walkSpeedMax = macroIn.WalkingSpeedMinMax.Y.ToString();
                walkSpeedMod = macroIn.WalkingSpeedModifier.ToString();
                walkingDistMod = macroIn.WalkingDistanceModifier.ToString();
                evacOrderTime = evacIn.EvacuationOrderStart.ToString();

            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int buttonColumnStart = 140;

            //
            popIn.AllowMoreThanOneCar = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), popIn.AllowMoreThanOneCar, "Allow more than one car");
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

            int.TryParse(maxCars, out popIn.MaxCars);
            float.TryParse(maxCarsProb, out popIn.MaxCarsProbability);
            int.TryParse(minHousehold, out popIn.MinHouseholdSize);
            int.TryParse(maxHousehold, out popIn.MaxHouseholdSize);
            float.TryParse(walkSpeedMin, out macroIn.WalkingSpeedMinMax.X);
            float.TryParse(walkSpeedMax, out macroIn.WalkingSpeedMinMax.Y);
            float.TryParse(walkSpeedMod, out macroIn.WalkingSpeedModifier);
            float.TryParse(walkingDistMod, out macroIn.WalkingDistanceModifier);
            float.TryParse(evacOrderTime, out evacIn.EvacuationOrderStart);
        }
    }
}
