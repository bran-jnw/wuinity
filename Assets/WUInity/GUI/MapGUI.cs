using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public partial class WUInityGUI
    {
        bool mapMenuDirty = true;
        string Lat, Long, sizeX, sizeY, zoom;

        void MapMenu()
        {
            WUInityInput wO = WUInity.INPUT;

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
                WUInity.INSTANCE.UpdateMapResourceStatus();
            }
        }

        void CleanMapMenu(WUInityInput wO)
        {
            mapMenuDirty = false;
            Lat = wO.lowerLeftLatLong.x.ToString();
            Long = wO.lowerLeftLatLong.y.ToString();
            sizeX = wO.size.x.ToString();
            sizeY = wO.size.y.ToString();
            zoom = wO.zoomLevel.ToString();
        }

        void ParseMapData(WUInityInput wO)
        {
            if (mapMenuDirty)
            {
                return;
            }

            double.TryParse(Lat, out wO.lowerLeftLatLong.x);
            double.TryParse(Long, out wO.lowerLeftLatLong.y);
            double.TryParse(sizeX, out wO.size.x);
            double.TryParse(sizeY, out wO.size.y);
            int.TryParse(zoom, out wO.zoomLevel);
        }
    }
}

