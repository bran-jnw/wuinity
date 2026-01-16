//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace PREACT.IO
{
    [System.Serializable]
    public class MapInput
    {
        public enum MapServiceProvider { Mapbox, Bing, OSM };

        public MapServiceProvider MapProvider = MapServiceProvider.Mapbox;
        public int ZoomLevel = 13;

        public MapInput() 
        { 
        }

        public void Parse(string[] inputLines, int startIndex, out bool success)
        {
            success = false;
            int issues = 0;            
            Dictionary<string, string> inputToParse = PREACTInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;
            
            input = nameof(MapProvider);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(MapServiceProvider.Mapbox):
                        MapProvider = MapServiceProvider.Mapbox;
                        break;
                    case nameof(MapServiceProvider.Bing):
                        MapProvider = MapServiceProvider.Bing;
                        break;
                    case nameof(MapServiceProvider.OSM):
                        MapProvider = MapServiceProvider.OSM;
                        break;
                    default:
                        ++issues;
                        Engine.Message(null, Engine.LogType.SimulationError, "Unknown map provider supplied by user, using " + MapProvider.ToString() + ".");
                        break;
                }
            }
            else
            {
            }

            input = nameof(ZoomLevel);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out ZoomLevel);
                if(ZoomLevel < 0 || ZoomLevel > 20)
                {
                    ZoomLevel = 13;
                    Engine.Message(null, Engine.LogType.Warning, "User has specified an incorrect zoom level (" + userInput + "), using " + ZoomLevel + ".");                                       
                }
            }
            else
            {
            }

            success = true;
        }
    }
}