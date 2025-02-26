//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Numerics;
using System.IO;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class PedestrianInput
    {
        public enum PedestrianModuleChoice { MacroHouseholdSim, JupedSimSUMO }
        public PedestrianModuleChoice pedestrianModuleChoice = PedestrianModuleChoice.MacroHouseholdSim;

        public MacroHouseholdSimInput macroHouseholdSimInput;

        public string[] BlockGoalEventFiles;
    }

    public class MacroHouseholdSimInput
    {
        public float walkingDistanceModifier = 1.0f;
        public Vector2 walkingSpeedMinMax = new Vector2(0.7f, 1.0f);
        public float walkingSpeedModifier = 1.0f;
    }
}
   