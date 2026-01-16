//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Evacuation;
using PREACT.Math;
using System.Collections.Generic;
using System.IO;

namespace PREACT.IO
{
    public class EvacuationData
    {

        public EvacuationData()
        {
        }           

        public void LoadAll(string rootFolder, out bool success)
        {
            success = false;
            Engine.Message(null, Engine.LogType.Log, "Loading Evacuation data...");
            success = true;
        }       
    }
}