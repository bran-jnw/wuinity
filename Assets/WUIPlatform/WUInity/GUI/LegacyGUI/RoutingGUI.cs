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
        string[] osmFilter = new string[] { ".pbf", ".osm" };
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
            if (tO.macroTrafficSimInput.routeChoice == MacroTrafficSimInput.RouteChoice.Fastest)
            {
                routeChoice += "Fastest";
            }
            else if (tO.macroTrafficSimInput.routeChoice == MacroTrafficSimInput.RouteChoice.Closest)
            {
                routeChoice += "Closest";
            }
            else if (tO.macroTrafficSimInput.routeChoice == MacroTrafficSimInput.RouteChoice.Random)
            {
                routeChoice += "Random";
            }
            else if (tO.macroTrafficSimInput.routeChoice == MacroTrafficSimInput.RouteChoice.EvacGroup)
            {
                routeChoice += "Evac. group";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routeChoice);
            ++buttonIndex;
        }
        void ParseRoutingInput()
        {
            if (routingMenuDirty)
            {
                return;
            }
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

