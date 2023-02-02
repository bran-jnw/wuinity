using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string borderSize;
        bool routingMenuDirty = true;

        string[] routerDbFilter = new string[] { ".routerdb" };
        string[] routeCollectionFilter = new string[] { ".rc" };
        string[] osmFilter = new string[] { ".pbf" };

        bool filterMenuActive = false;

        void RoutingMenu()
        {
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            TrafficInput tO = WUInity.INPUT.Traffic;
            if (routingMenuDirty)
            {
                routingMenuDirty = false;
                borderSize = WUInity.RUNTIME_DATA.Routing.BorderSize.ToString();
            }

            //router db
            string routerStatus = "RouterDb NOT loaded";
            if (WUInity.DATA_STATUS.RouterDbLoaded)
            {
                routerStatus = "RouterDb loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routerStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load RouterDb"))
            {
                OpenLoadRouterDbFile();
            }
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM -> RouterDb"))
            {
                OpenBuildRouterDbFromOSM();
            }
            ++buttonIndex;

            //route choice info
            ++buttonIndex;
            string routeChoice = "Route choice: ";
            if (tO.routeChoice == TrafficInput.RouteChoice.Fastest)
            {
                routeChoice += "Fastest";
            }
            else if (tO.routeChoice == TrafficInput.RouteChoice.Closest)
            {
                routeChoice += "Closest";
            }
            else if (tO.routeChoice == TrafficInput.RouteChoice.Random)
            {
                routeChoice += "Random";
            }
            else if (tO.routeChoice == TrafficInput.RouteChoice.EvacGroup)
            {
                routeChoice += "Evac. group";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routeChoice);
            ++buttonIndex;

            //route collection
            string routeCollectionStatus = "Route collection NOT loaded";
            if (WUInity.DATA_STATUS.RouteCollectionLoaded)
            {
                routeCollectionStatus = "Route collection loaded";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routeCollectionStatus);
            ++buttonIndex;


            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load route collection"))
            {
                OpenLoadRouteCollectionFile();
            }
            ++buttonIndex;

            if (WUInity.DATA_STATUS.RouterDbLoaded)
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Build route collection"))
                {
                    WUInity.RUNTIME_DATA.Routing.BuildAndSaveRouteCollection();
                }
                ++buttonIndex;
            }

            //OSM stuff
            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Filter OSM data"))
            {
                filterMenuActive = true;
                WUInity.INSTANCE.SetOSMBorderVisibility(true);
            }
            ++buttonIndex;

            if (filterMenuActive)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM border size [m]");
                ++buttonIndex;
                borderSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), borderSize);
                float.TryParse(borderSize, out WUInity.RUNTIME_DATA.Routing.BorderSize);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Select OSM file"))
                {
                    WUInity.INSTANCE.SetOSMBorderVisibility(false);
                    ParseRoutingInput();
                    OpenFilterOSMFile();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cancel"))
                {
                    WUInity.INSTANCE.SetOSMBorderVisibility(false);
                    filterMenuActive = false;
                }
                ++buttonIndex;
            }
        }
        void ParseRoutingInput()
        {
            if (routingMenuDirty)
            {
                return;
            }

            float.TryParse(borderSize, out WUInity.RUNTIME_DATA.Routing.BorderSize);
        }

        void OpenLoadRouterDbFile()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadRouterDbFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select RouterDb", "Load");
        }

        void LoadRouterDbFile(string[] paths)
        {
            string selectedFile = paths[0];            
            WUInity.RUNTIME_DATA.Routing.LoadRouterDb(selectedFile, true);
        }

        void OpenLoadRouteCollectionFile()
        {
            FileBrowser.SetFilters(false, routeCollectionFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadRouteCollectionFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select Router collection", "Load");
        }

        void LoadRouteCollectionFile(string[] paths)
        {
            string selectedFile = paths[0];
            WUInity.RUNTIME_DATA.Routing.LoadRouteCollection(selectedFile, true);
        }

        void OpenFilterOSMFile()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(FilterOSMFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select source OSM file", "Filter");
        }

        void FilterOSMFile(string[] paths)
        {
            string selectedFile = paths[0];
            WUInity.RUNTIME_DATA.Routing.FilterOSMData(selectedFile);
        }

        void OpenBuildRouterDbFromOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(BuildRouterDbFromOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select source OSM file", "Build");
        }

        void BuildRouterDbFromOSM(string[] paths)
        {
            string selectedFile = paths[0];
            WUInity.RUNTIME_DATA.Routing.CreateRouterDatabaseFromOSM(selectedFile);
        }
    }
}

