//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace WUIPlatform.IO
{
    [System.Serializable]
    public class MapInput
    {
        public enum MapServiceProvider { Mapbox, Bing, OSM };

        public MapServiceProvider MapProvider = MapServiceProvider.Mapbox;
        public int ZoomLevel = 13;

        public static MapInput Parse(string[] inputLines, int startIndex)
        {
            int issues = 0;
            var newInput = new MapInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string input, userInput;
            
            input = nameof(MapProvider);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                switch (userInput)
                {
                    case nameof(MapServiceProvider.Mapbox):
                        newInput.MapProvider = MapServiceProvider.Mapbox;
                        break;
                    case nameof(MapServiceProvider.Bing):
                        newInput.MapProvider = MapServiceProvider.Bing;
                        break;
                    case nameof(MapServiceProvider.OSM):
                        newInput.MapProvider = MapServiceProvider.OSM;
                        break;
                    default:
                        ++issues;
                        WUIEngine.LOG(WUIEngine.LogType.SimError, "Unknown map provider supplied by user, using " + newInput.MapProvider.ToString() + ".");
                        break;
                }
            }
            else
            {
            }

            input = nameof(ZoomLevel);
            if (inputToParse.TryGetValue(input, out userInput))
            {
                int.TryParse(userInput, out newInput.ZoomLevel);
                if(newInput.ZoomLevel < 0 || newInput.ZoomLevel > 20)
                {
                    WUIEngine.LOG(WUIEngine.LogType.Warning, "User has specified an incorrect zoom level (" + userInput + "), using " + newInput.ZoomLevel + ".");
                    newInput.ZoomLevel = 13;                   
                }
            }
            else
            {
            }

            return newInput;
        }
    }
}