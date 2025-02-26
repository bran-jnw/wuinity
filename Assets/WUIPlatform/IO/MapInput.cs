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

        public MapServiceProvider mapProvider = MapServiceProvider.Mapbox;
        public int zoomLevel = 13;

        public static MapInput Parse(string[] inputLines, int startIndex)
        {
            var newInput = new MapInput();
            Dictionary<string, string> inputToParse = WUIEngineInput.GetHeaderInput(inputLines, startIndex);
            string temp;
            int issues = 0;

            if (inputToParse.TryGetValue(nameof(mapProvider), out temp))
            {
                switch (temp)
                {
                    case nameof(MapServiceProvider.Mapbox):
                        newInput.mapProvider = MapServiceProvider.Mapbox;
                        break;
                    case nameof(MapServiceProvider.Bing):
                        newInput.mapProvider = MapServiceProvider.Bing;
                        break;
                    case nameof(MapServiceProvider.OSM):
                        newInput.mapProvider = MapServiceProvider.OSM;
                        break;
                    default:
                        newInput.mapProvider = MapServiceProvider.Mapbox;
                        break;
                }
            }
            else
            {
            }

            if (inputToParse.TryGetValue(nameof(zoomLevel), out temp))
            {
                int.TryParse(temp, out newInput.zoomLevel);
            }
            else
            {
            }



            return newInput;
        }
    }
}