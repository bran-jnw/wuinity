//This file is part of WUIPlatform Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Utility.Math;

namespace PREACT.Utility
{
    public static class ShapeFileReader
    {        

        public static void GetShapefileExtentsAllLayers(string path, out Vector2d min, out Vector2d max)
        {
            min = new Vector2d(double.MaxValue, double.MaxValue);
            max = new Vector2d(double.MinValue, double.MinValue);
            OSGeo.OGR.Driver driver = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile");
            OSGeo.OGR.Envelope extent = new OSGeo.OGR.Envelope();
            var shapeFile = driver.Open(path, 0);
            var layerCount = shapeFile.GetLayerCount();
            
            for (int i = 0; i < layerCount; i++)
            {
                var layer = shapeFile.GetLayerByIndex(i);
                layer.GetExtent(extent, 1);
                if(extent.MinX < min.x)
                {
                    min.x = extent.MinX;
                }

                if (extent.MaxX > max.x)
                {
                    max.x = extent.MaxX;
                }

                if (extent.MinY < min.y)
                {
                    min.y = extent.MinY;
                }

                if (extent.MaxY > max.y)
                {
                    max.y = extent.MaxY;
                }
            }

            driver.Deregister();
        }
    }
}

