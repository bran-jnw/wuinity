using UnityEngine;
using PREACT.IO;
using System.IO;
using PREACT.Evacuation;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string totalPop, maxCars, maxCarsProb, walkingDistMod, walkSpeedMin, walkSpeedMax, walkSpeedMod, evacOrderTime;
        bool evacMenuDirty = true;

        void EvacMenu()
        {
            PopulationInput popIn = _input.Population;
            MacroHouseholdSimInput macroIn = _input.Pedestrian.MacroHouseholdSimInput;
            EvacuationInput evacIn = _input.Evacuation;

            if (evacMenuDirty)
            {
                evacMenuDirty = false;
                maxCars = popIn.MaxCars.ToString();
                maxCarsProb = popIn.MaxCarsProbability.ToString();
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

            if (!_wuinityManager.IsPainterActive())
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Edit evac group"))
                {
                    _wuinityManager.StartPainter(Painter.PaintMode.EvacGroup);
                }
                ++buttonIndex;                
            }
            else
            {
                for (int i = 0; i < _input.Evacuation.Data.EvacuationGroups.Count; i++)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), _input.Evacuation.Data.EvacuationGroups[i].Name))
                    {
                        _wuinityManager.Painter.SetEvacGroupColor(i);
                    }
                    ++buttonIndex;
                }

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Finish editing"))
                {
                    string filePath = Path.Combine(_input.RootFolder, _input.Simulation.Name + ".egs");
                    EvacuationGroup.SaveEvacGroupIndices(filePath, _input.Evacuation.Data.CellCount, _input.Evacuation.Data.EvacuationGroups.Count, _input.Evacuation.Data.EvacGroupIndices);
                    _wuinityManager.StopPainter();
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

            PopulationInput popIn = _input.Population;
            MacroHouseholdSimInput macroIn = _input.Pedestrian.MacroHouseholdSimInput;
            EvacuationInput evacIn = _input.Evacuation;

            int.TryParse(maxCars, out popIn.MaxCars);
            float.TryParse(maxCarsProb, out popIn.MaxCarsProbability);
            float.TryParse(walkSpeedMin, out macroIn.WalkingSpeedMinMax.X);
            float.TryParse(walkSpeedMax, out macroIn.WalkingSpeedMinMax.Y);
            float.TryParse(walkSpeedMod, out macroIn.WalkingSpeedModifier);
            float.TryParse(walkingDistMod, out macroIn.WalkingDistanceModifier);
            float.TryParse(evacOrderTime, out evacIn.EvacuationOrderStart);
        }
    }
}
