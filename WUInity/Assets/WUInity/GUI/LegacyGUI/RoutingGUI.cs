using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using PREACT.Input;
using PREACT.Tools;

namespace WUInity.UI
{
    public partial class WUInityGUI
    {
        string borderSize;
        bool routingMenuDirty = true;

        string[] routerDbFilter = new string[] { ".routerdb" };
        string[] routeCollectionFilter = new string[] { ".rc" };
        string[] osmFilter = new string[] { ".pbf", ".osm", ".xml" };
        string[] maskFilter = new string[] { ".pmk" };
        string[] csvFilter = new string[] { ".csv" };

        bool filterMenuActive = false;

        void RoutingMenu()
        {
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;
            TrafficModuleInput tO = _input.TrafficModule;
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

            if(_input.TrafficModule.MacroTrafficSimInput != null)
            {
                //route choice info
                ++buttonIndex;
                string routeChoice = "Route choice: ";
                if (tO.MacroTrafficSimInput.Routing == MacroTrafficSimInput.RoutingPriority.Fastest)
                {
                    routeChoice += "Fastest";
                }
                else if (tO.MacroTrafficSimInput.Routing == MacroTrafficSimInput.RoutingPriority.Closest)
                {
                    routeChoice += "Closest";
                }
                else if (tO.MacroTrafficSimInput.Routing == MacroTrafficSimInput.RoutingPriority.Random)
                {
                    routeChoice += "Random";
                }
                else if (tO.MacroTrafficSimInput.Routing == MacroTrafficSimInput.RoutingPriority.EvacGroup)
                {
                    routeChoice += "Evac. group";
                }
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routeChoice);
                ++buttonIndex;
            }            
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
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowLoadDialog(LoadRouterDbFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select RouterDb", "Load");
        }

        void LoadRouterDbFile(string[] paths)
        {
            bool success;
            //PopulationTools.LoadRouterDb(paths[0], out success);
        }

        void OpenBuildRouterDbFromOSM()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(_engine.WorkingFile);
            FileBrowser.ShowLoadDialog(BuildRouterDbFromOSM, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Select source OSM file", "Build");
        }

        void BuildRouterDbFromOSM(string[] paths)
        {
            string outputFile = Path.Combine(Path.GetDirectoryName(paths[0]), Path.GetFileNameWithoutExtension(paths[0]), ".routerDb");
            PopulationTools.CreateAndSaveRouterDb(paths[0], outputFile, out bool success);
        }
    }
}

