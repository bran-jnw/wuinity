using UnityEngine;
using SimpleFileBrowser;
using System.IO;

namespace WUInity
{
    public partial class WUInityGUI
    {
        string opticalDensity, stallSpeed, borderSize;
        bool trafficMenuDirty = true;
        string[] opticalDensityFilter = new string[] { ".odr" };

        void TrafficMenu()
        {
            TrafficInput tO = WUInity.INPUT.traffic;
            if (trafficMenuDirty)
            {
                trafficMenuDirty = false;
                stallSpeed = tO.stallSpeed.ToString();
                opticalDensity = tO.opticalDensity.ToString();
                borderSize = tO.osmBorderSize.ToString();
            }
            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            //router db
            string routerStatus = "RouterDb NOT loaded";
            string loadRouterText = "Load router database";
            if (WUInity.DATA_STATUS.RouterDbLoaded)
            {
                routerStatus = "RouterDb loaded";
                loadRouterText = "Re-load router database";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routerStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), loadRouterText))
            {
                ParseTrafficInput();
                WUInity.INSTANCE.UpdateRoutingResourceStatus();
            }
            ++buttonIndex;

            //route collection
            ++buttonIndex;
            string routeCollectionStatus = "Pre-calc routes NOT loaded";
            string loadRouteCollectionsText = "Load pre-calc routes";
            if (WUInity.DATA_STATUS.RouteCollectionLoaded)
            {                
                routeCollectionStatus = "Pre-calc routes loaded";
                loadRouteCollectionsText = "Re-load pre-calc routes";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), routeCollectionStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), loadRouteCollectionsText))
            {
                ParseTrafficInput();
                WUInity.DATA_STATUS.RouteCollectionLoaded =  WUInity.SIM_DATA.LoadRouteCollections();
            }
            ++buttonIndex;

            //OSM
            ++buttonIndex;
            string osmStatus = "OSM file NOT valid";
            if (WUInity.DATA_STATUS.OsmFileValid)
            {
                osmStatus = "OSM file valid";
            }
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), osmStatus);
            ++buttonIndex;

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Select OSM source file"))
            {
                OpenSetOSMFile();
            }
            ++buttonIndex;            
            
            if(WUInity.DATA_STATUS.OsmFileValid)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "OSM border size [m]");
                ++buttonIndex;
                borderSize = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), borderSize);
                ++buttonIndex;

                string buildRouterText = "Build router database";
                if(WUInity.DATA_STATUS.RouterDbLoaded)
                {
                    buildRouterText = "Re-build router database";
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), buildRouterText))
                {
                    File.Delete(Path.Combine(WUInity.WORKING_FOLDER, WUInity.INPUT.simName + ".routerdb"));
                    WUInity.SIM_DATA.LoadRouterDatabase();
                }
                ++buttonIndex;
            }

            //settings
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Capacity speed");
            ++buttonIndex;
            stallSpeed = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), stallSpeed);
            ++buttonIndex;

            //smoke stuff
            tO.visibilityAffectsSpeed = GUI.Toggle(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), tO.visibilityAffectsSpeed, "Speed is affected by smoke");
            ++buttonIndex;

            if(tO.visibilityAffectsSpeed)
            {
                GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density [1/m]");
                ++buttonIndex;
                opticalDensity = GUI.TextField(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), opticalDensity);
                ++buttonIndex;

                if (WUInity.DATA_STATUS.OpticalDensityLoaded)
                {
                    GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Optical density ramp loaded");
                    ++buttonIndex;
                }
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load optical density"))
                {
                    OpenLoadOpticalDensityFile();
                }
                ++buttonIndex;
            } 

            if (WUInity.INSTANCE.DeveloperMode)
            {
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Run traffic verification"))
                {
                    Traffic.MacroTrafficVerification.RunTrafficVerificationTests();
                }
                ++buttonIndex;
            }
        }

        void ParseTrafficInput()
        {
            if (trafficMenuDirty)
            {
                return;
            }

            TrafficInput tO = WUInity.INPUT.traffic;
            ItineroInput iO = WUInity.INPUT.itinero;

            float.TryParse(stallSpeed, out tO.stallSpeed);
            float.TryParse(opticalDensity, out tO.opticalDensity);
            float.TryParse(borderSize, out tO.osmBorderSize);
        }

        void OpenSetOSMFile()
        {
            FileBrowser.SetFilters(false, osmFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(SetOSMFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Set OSM file", "Set");
        }

        void SetOSMFile(string[] paths)
        {
            WUInity.INPUT.traffic.osmFile = paths[0];
            WUInity.INSTANCE.UpdateOSMResourceStatus();
        }

        void OpenLoadOpticalDensityFile()
        {
            FileBrowser.SetFilters(false, opticalDensityFilter);
            string initialPath = Path.GetDirectoryName(WUInity.WORKING_FILE);
            FileBrowser.ShowLoadDialog(LoadOpticalDensityFile, CancelSaveLoad, FileBrowser.PickMode.Files, false, initialPath, null, "Load optical density ramp file", "Load");
        }

        void LoadOpticalDensityFile(string[] paths)
        {
            WUInityInput wO = WUInity.INPUT;
            wO.traffic.opticalDensityFile = paths[0];
            WUInity.SIM_DATA.LoadOpticalDensityFile();
        }
    }
}
