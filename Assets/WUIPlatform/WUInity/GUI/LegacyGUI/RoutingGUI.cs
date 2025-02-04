using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using WUIPlatform.IO;

namespace WUIPlatform.WUInity.UI
{
    public partial class WUInityGUI
    {
        string borderSize;
        bool routingMenuDirty = true;

        string[] routerDbFilter = new string[] { ".routerdb" };
        string[] routeCollectionFilter = new string[] { ".rc" };
        string[] osmFilter = new string[] { ".pbf" };
        string[] maskFilter = new string[] { ".pmk" };

        bool filterMenuActive = false;

        void RoutingMenu()
        {
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            TrafficInput tO = WUIEngine.INPUT.Traffic;
            if (routingMenuDirty)
            {
                routingMenuDirty = false;
                borderSize = _osmBorderSize.x.ToString();
            }

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

            //OSM stuff
            ++buttonIndex;
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Filter OSM data"))
            {
                filterMenuActive = true;
                WUInityEngine.INSTANCE.SetOSMBorderVisibility(true);
            }
            ++buttonIndex;

            if (filterMenuActive)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM border size [m]");
                ++buttonIndex;
                borderSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), borderSize);
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Select OSM file"))
                {
                    WUInityEngine.INSTANCE.SetOSMBorderVisibility(false);
                    ParseRoutingInput();
                    OpenFilterOSMFile();
                }
                ++buttonIndex;

                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Cancel"))
                {
                    WUInityEngine.INSTANCE.SetOSMBorderVisibility(false);
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

            double.TryParse(borderSize, out _osmBorderSize.x);
            double.TryParse(borderSize, out _osmBorderSize.y);
        }

        void OpenLoadRouterDbFile()
        {
            FileBrowser.SetFilters(false, routerDbFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadRouterDbFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select RouterDb", "Load");
        }

        void LoadRouterDbFile(string[] paths)
        {
            string selectedFile = paths[0];
            Tools.PopulationTools.LoadRouterDb(selectedFile);
        }

        void OpenFilterOSMFile()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(FilterOSMFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select source OSM file", "Filter");
        }

        float _interpolatedCellSize = 100f;
        Vector2d _osmBorderSize = new Vector2d(2000, 2000);
        void FilterOSMFile(string[] paths)
        {
            string selectedFile = paths[0];
            Tools.PopulationTools.FilterOsmData(selectedFile, _osmBorderSize);
        }

        void OpenBuildRouterDbFromOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUIEngine.WORKING_FILE);
            FileBrowser.ShowLoadDialog(BuildRouterDbFromOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select source OSM file", "Build");
        }

        void BuildRouterDbFromOSM(string[] paths)
        {
            string selectedFile = paths[0];
            Tools.PopulationTools.CreateAndSaveRouterDb(selectedFile);
        }
    }
}

