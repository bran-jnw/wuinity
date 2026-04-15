using UnityEngine;
using PREACT.Math;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        bool mapMenuDirty = true;
        string Lat, Long, sizeX, sizeY, zoom;

        void MapMenu()
        {
            //whenever we load a file we need to set the new data for the GUI
            if (mapMenuDirty)
            {
                CleanMapMenu();
            }

            GUI.Box(new Rect(subMenuXOrigin, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            //LatLong
            GUI.Label(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map LL Lat.:");
            ++buttonIndex;
            Lat = GUI.TextField(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), Lat);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map LL Long.:");
            ++buttonIndex;
            Long = GUI.TextField(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), Long);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map size x [m]:");
            ++buttonIndex;
            sizeX = GUI.TextField(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), sizeX);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map size y [m]:");
            ++buttonIndex;
            sizeY = GUI.TextField(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), sizeY);
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Map zoom level:");
            ++buttonIndex;
            zoom = GUI.TextField(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), zoom);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex* (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Update map"))
            {
                ParseMapData();                
            }
        }

        void CleanMapMenu()
        {            
            Lat = _input.Simulation.LowerLeftLatLon.x.ToString();
            Long = _input.Simulation.LowerLeftLatLon.y.ToString();
            sizeX = _input.Simulation.DomainSize.x.ToString();
            sizeY = _input.Simulation.DomainSize.y.ToString();
            zoom = _input.Map.ZoomLevel.ToString();
            mapMenuDirty = false;
        }

        void ParseMapData()
        {
            if (mapMenuDirty)
            {
                return;
            }

            int issues = 0;
            Vector2d temp;
            issues += double.TryParse(Lat, out temp.x) ? 0 : 1;
            issues += double.TryParse(Long, out temp.y) ? 0 : 1;
            if(issues == 0)
            {
                _input.Simulation.LowerLeftLatLon = temp;
                //_wuinityManager.SetInputMap();
            }           
            double.TryParse(sizeX, out _input.Simulation.DomainSize.x);
            double.TryParse(sizeY, out _input.Simulation.DomainSize.y);
            int.TryParse(zoom, out _input.Map.ZoomLevel);
        }
    }
}

