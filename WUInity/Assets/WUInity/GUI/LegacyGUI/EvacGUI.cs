using UnityEngine;
using PREACT.Input;
using System.IO;
using PREACT.Evacuation;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string totalPop, walkingDistMod, walkSpeedMin, walkSpeedMax, walkSpeedMod, evacOrderTime;
        bool evacMenuDirty = true;

        void EvacMenu()
        {
            PopulationInput popIn = _input.Population;
            MacroHouseholdSimInput macroIn = _input.PedestrianModule.MacroHouseholdSimInput;
            EvacuationInput evacIn = _input.Evacuation;

            if (evacMenuDirty)
            {
                evacMenuDirty = false;
                walkSpeedMin = macroIn.WalkingSpeedMinMax.X.ToString();
                walkSpeedMax = macroIn.WalkingSpeedMinMax.Y.ToString();
                walkSpeedMod = macroIn.WalkingSpeedModifier.ToString();
                walkingDistMod = macroIn.WalkingDistanceModifier.ToString();

            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            int buttonColumnStart = 140;
                                    
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
        }

        void ParseEvacInput()
        {
            if (evacMenuDirty)
            {
                return;
            }

            PopulationInput popIn = _input.Population;
            MacroHouseholdSimInput macroIn = _input.PedestrianModule.MacroHouseholdSimInput;
            EvacuationInput evacIn = _input.Evacuation;

            float.TryParse(walkSpeedMin, out macroIn.WalkingSpeedMinMax.X);
            float.TryParse(walkSpeedMax, out macroIn.WalkingSpeedMinMax.Y);
            float.TryParse(walkSpeedMod, out macroIn.WalkingSpeedModifier);
            float.TryParse(walkingDistMod, out macroIn.WalkingDistanceModifier);
        }
    }
}
