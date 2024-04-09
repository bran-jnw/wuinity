using UnityEngine;
using WUIEngine.IO;
using WUIEngine;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        bool mapMenuDirty = true;
        string Lat, Long, sizeX, sizeY, zoom;

        void MapMenu()
        {
            WUInityInput wO = Engine.INPUT;

            //whenever we load a file we need to set the new data for the GUI
            if (mapMenuDirty)
            {
                CleanMapMenu(wO);
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
                ParseMapData(wO);
                Engine.ENGINE.UpdateMapResourceStatus();
            }
        }

        void CleanMapMenu(WUInityInput wO)
        {
            mapMenuDirty = false;
            Lat = wO.Simulation.LowerLeftLatLong.x.ToString();
            Long = wO.Simulation.LowerLeftLatLong.y.ToString();
            sizeX = wO.Simulation.Size.x.ToString();
            sizeY = wO.Simulation.Size.y.ToString();
            zoom = wO.Map.zoomLevel.ToString();
        }

        void ParseMapData(WUInityInput wO)
        {
            if (mapMenuDirty)
            {
                return;
            }

            double.TryParse(Lat, out wO.Simulation.LowerLeftLatLong.x);
            double.TryParse(Long, out wO.Simulation.LowerLeftLatLong.y);
            double.TryParse(sizeX, out wO.Simulation.Size.x);
            double.TryParse(sizeY, out wO.Simulation.Size.y);
            int.TryParse(zoom, out wO.Map.zoomLevel);
        }
    }
}

