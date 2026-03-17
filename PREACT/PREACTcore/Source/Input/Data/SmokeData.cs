//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Input;
using System.IO;

namespace PREACT.Input
{
    public class SmokeData
    {
        private Dispersion.ExtinctionRamp _extinctionRamp;

        public Dispersion.ExtinctionRamp ExtinctionRamp { get => _extinctionRamp; }

        public void LoadAll(SmokeInput smokeInput, string rootFolder, out bool success)
        {
            success = false;

            if (smokeInput.Module == SmokeInput.SmokeModules.GlobalSmoke)
            {
                string filePath = Path.Combine(rootFolder, smokeInput.GlobalSmokeInput.ExtinctionFile);
                LoadExtinctionRamp(filePath, out success);
            }
            else
            {
                success = true;
            }
        }

        public void LoadExtinctionRamp(string filePath, out bool success)
        {
            _extinctionRamp = Dispersion.ExtinctionRamp.LoadExtinctionRampFile(filePath, out success);
        }
    }

    
}